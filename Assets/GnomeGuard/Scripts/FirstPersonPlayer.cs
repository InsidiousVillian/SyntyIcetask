using UnityEngine;
using UnityEngine.InputSystem;

namespace GnomeGuard
{
    public class FirstPersonPlayer : MonoBehaviour
    {
        public CharacterController Controller;
        public Transform Head;
        public Camera Camera;
        public float MoveSpeed = 7.2f;
        public float SprintMultiplier = 1.32f;
        public float MouseSensitivity = 0.12f;
        public float ThrowForce = 26f;
        public float BaseCooldown = 0.34f;

        public float Charge01 { get; private set; }
        public bool CanAct { get; set; }

        float _pitch;
        float _yaw;
        float _verticalVelocity;
        float _nextThrow;
        float _nextKick;
        float _bob;
        float _fovVel;
        float _baseFov = 70f;
        float _targetFov = 70f;
        float _titleOrbit;
        Vector3 _spawnPos;
        Quaternion _spawnRot;
        Transform _viewmodel;
        Vector3 _viewmodelHome;
        float _viewHideUntil;
        float _lastStep;
        bool _charging;

        public void CaptureSpawn()
        {
            _spawnPos = transform.position;
            _spawnRot = transform.rotation;
            _yaw = transform.eulerAngles.y;
        }

        public void ResetToSpawn()
        {
            if (Controller != null) Controller.enabled = false;
            transform.position = _spawnPos;
            transform.rotation = _spawnRot;
            if (Controller != null) Controller.enabled = true;
            _yaw = _spawnRot.eulerAngles.y;
            _pitch = 8f;
            _verticalVelocity = -2f;
            if (Head != null)
            {
                Head.localPosition = new Vector3(0f, 1.62f, 0f);
                Head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }

            if (Camera != null)
            {
                Camera.fieldOfView = _baseFov;
                Camera.transform.localRotation = Quaternion.identity;
            }

            Charge01 = 0f;
            _charging = false;
            _targetFov = _baseFov;
        }

        void Start()
        {
            CaptureSpawn();
            if (Camera != null) _baseFov = Camera.fieldOfView;
            BuildViewmodel();
        }

        void BuildViewmodel()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "ViewSnowball";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(Camera.transform, false);
            go.transform.localPosition = new Vector3(0.28f, -0.2f, 0.48f);
            go.transform.localScale = Vector3.one * 0.16f;
            go.GetComponent<MeshRenderer>().sharedMaterial = GnomeAssets.SnowballMaterial();
            _viewmodel = go.transform;
            _viewmodelHome = go.transform.localPosition;
        }

        void Update()
        {
            if (GnomeGuardGame.Instance == null) return;

            if (!GnomeGuardGame.Instance.IsPlaying && !GnomeGuardGame.Instance.IsGameOver)
            {
                TitleOrbit();
                return;
            }

            if (!CanAct || GnomeGuardGame.Instance.Paused)
                return;

            Look();
            Move();
            ThrowSnowballs();
            Kick();
            AnimateViewmodel();
            UpdateFov();
        }

        void TitleOrbit()
        {
            _titleOrbit += Time.deltaTime * 16f;
            float rad = _titleOrbit * Mathf.Deg2Rad;
            transform.position = new Vector3(Mathf.Sin(rad) * 8.6f, 0.08f, Mathf.Cos(rad) * 8.6f);
            Vector3 look = -new Vector3(transform.position.x, 0f, transform.position.z);
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(look);
            Head.localRotation = Quaternion.Euler(10f, 0f, 0f);
            if (Camera != null) Camera.fieldOfView = 62f;
            if (_viewmodel != null) _viewmodel.gameObject.SetActive(false);
        }

        void Look()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 delta = mouse.delta.ReadValue();
            _yaw += delta.x * MouseSensitivity;
            _pitch -= delta.y * MouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, -82f, 82f);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            Head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        void Move()
        {
            var kb = Keyboard.current;
            Vector2 input = Vector2.zero;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            }

            Vector3 planar = (transform.right * input.x + transform.forward * input.y);
            if (planar.sqrMagnitude > 1f) planar.Normalize();

            bool sprint = kb != null && kb.leftShiftKey.isPressed && planar.sqrMagnitude > 0.1f;
            float speed = MoveSpeed * (sprint ? SprintMultiplier : 1f);

            if (Controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (kb != null && kb.spaceKey.wasPressedThisFrame && Controller.isGrounded)
            {
                _verticalVelocity = 7.2f;
                GameSfx.Jump();
            }

            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            Vector3 velocity = planar * speed + Vector3.up * _verticalVelocity;
            Controller.Move(velocity * Time.deltaTime);

            float speed01 = new Vector3(velocity.x, 0f, velocity.z).magnitude / MoveSpeed;
            _bob += Time.deltaTime * (8.5f + speed01 * 4f);
            float bobAmt = Controller.isGrounded ? speed01 * 0.045f : 0f;
            Head.localPosition = new Vector3(
                Mathf.Sin(_bob * 0.5f) * bobAmt,
                1.62f + Mathf.Abs(Mathf.Sin(_bob)) * bobAmt,
                0f);

            if (Camera != null)
            {
                float roll = -input.x * 2.2f;
                Camera.transform.localRotation = Quaternion.Euler(0f, 0f, roll);
            }

            _targetFov = _baseFov + (sprint ? 8f : 0f) + Charge01 * 4f;

            if (speed01 > 0.25f && Controller.isGrounded && Time.time >= _lastStep)
            {
                _lastStep = Time.time + (sprint ? 0.28f : 0.38f);
                GameSfx.Footstep();
                HitFx.Spawn(transform.position + transform.forward * 0.3f, new Color(0.9f, 0.95f, 1f, 0.5f), 6);
            }
        }

        void ThrowSnowballs()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            bool rapid = GnomeGuardGame.Instance != null && GnomeGuardGame.Instance.RapidFire;

            if (rapid)
            {
                _charging = false;
                Charge01 = 0f;
                if (!mouse.leftButton.isPressed) return;
                Fire(0f);
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
                _charging = true;

            if (_charging && mouse.leftButton.isPressed)
                Charge01 = Mathf.MoveTowards(Charge01, 1f, Time.deltaTime / 0.7f);
            else if (!_charging)
                Charge01 = Mathf.MoveTowards(Charge01, 0f, Time.deltaTime * 3f);

            if (_charging && mouse.leftButton.wasReleasedThisFrame)
            {
                Fire(Charge01);
                _charging = false;
                Charge01 = 0f;
            }
        }

        void Fire(float charge)
        {
            if (Time.time < _nextThrow) return;
            bool charged = charge >= 0.22f;
            _nextThrow = Time.time + BaseCooldown * (charged ? 1.15f : 1f);

            Vector3 origin = Camera.transform.position + Camera.transform.forward * 0.85f;
            float force = ThrowForce * (charged ? 1.15f + charge * 0.45f : 1f);
            Snowball.Spawn(origin, Camera.transform.forward, force, transform, charge);
            GameSfx.Throw(charged);
            if (GnomeGuardGame.Instance != null && GnomeGuardGame.Instance.Shake != null)
                GnomeGuardGame.Instance.Shake.Punch(charged ? 0.22f : 0.1f);
            _targetFov += charged ? 6f : 3f;
            _viewHideUntil = Time.time + 0.16f;
        }

        void Kick()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!(kb.fKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.qKey.wasPressedThisFrame))
                return;
            if (Time.time < _nextKick) return;
            _nextKick = Time.time + 0.85f;

            GameSfx.Kick();
            if (GnomeGuardGame.Instance != null && GnomeGuardGame.Instance.Shake != null)
                GnomeGuardGame.Instance.Shake.Punch(0.28f);

            Vector3 center = transform.position + transform.forward * 1.1f + Vector3.up * 0.8f;
            HitFx.Spawn(center, Color.white, 16);
            var hits = Physics.OverlapSphere(center, 1.5f);
            foreach (var hit in hits)
            {
                var target = hit.GetComponentInParent<IHittable>();
                if (target != null)
                    target.TryHit(center, transform.forward * 16f + Vector3.up * 2f, 1);
            }
        }

        void AnimateViewmodel()
        {
            if (_viewmodel == null) return;
            bool show = Time.time >= _viewHideUntil;
            _viewmodel.gameObject.SetActive(show);
            if (!show) return;

            float scale = 0.16f + Charge01 * 0.18f;
            _viewmodel.localScale = Vector3.one * scale;
            _viewmodel.localPosition = _viewmodelHome + new Vector3(0f, Charge01 * 0.05f, Charge01 * -0.04f);
            _viewmodel.Rotate(12f * Time.deltaTime, 30f * Time.deltaTime, 8f * Time.deltaTime, Space.Self);
        }

        void UpdateFov()
        {
            if (Camera == null) return;
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, _targetFov, ref _fovVel, 0.08f);
            _targetFov = Mathf.Lerp(_targetFov, _baseFov, Time.deltaTime * 3f);
        }
    }
}
