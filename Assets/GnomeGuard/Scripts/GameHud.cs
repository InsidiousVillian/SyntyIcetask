using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GnomeGuard
{
    public class GameHud : MonoBehaviour
    {
        Text _score;
        Text _wave;
        Text _tree;
        Text _objective;
        Text _status;
        Text _banner;
        Text _crosshair;
        Text _hint;
        Text _coach;
        Text _chargeLabel;
        Image _treeFill;
        Image _flash;
        Image _chargeFill;
        Image _hit;
        GameObject _titleRoot;
        GameObject _overRoot;
        GameObject _pauseRoot;
        GameObject _chargeRoot;
        GameObject _playRoot;
        Text _overBody;
        float _bannerUntil;
        float _flashUntil;
        float _hitUntil;
        Vector3 _crosshairScale = Vector3.one;

        public static GameHud Create(Transform parent)
        {
            EnsureEventSystem(parent);

            var canvasGo = new GameObject("HUD");
            canvasGo.transform.SetParent(parent, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var hud = canvasGo.AddComponent<GameHud>();
            hud.Build();
            return hud;
        }

        static void EnsureEventSystem(Transform parent)
        {
            var existing = Object.FindFirstObjectByType<EventSystem>();
            GameObject go;
            if (existing == null)
            {
                go = new GameObject("EventSystem");
                go.transform.SetParent(parent, false);
                go.AddComponent<EventSystem>();
            }
            else
            {
                go = existing.gameObject;
            }

            var legacy = go.GetComponent("StandaloneInputModule") as Component;
            if (legacy != null) Object.Destroy(legacy);
            if (go.GetComponent<InputSystemUIInputModule>() == null)
                go.AddComponent<InputSystemUIInputModule>();
        }

        void Build()
        {
            _playRoot = new GameObject("PlayHud");
            _playRoot.transform.SetParent(transform, false);
            Stretch(_playRoot.AddComponent<RectTransform>());

            _score = Label(_playRoot.transform, "Score", new Vector2(48f, -36f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 38);
            _wave = Label(_playRoot.transform, "Wave", new Vector2(48f, -88f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 26);
            _status = Label(_playRoot.transform, "Status", new Vector2(48f, -128f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 22);
            _status.color = new Color(1f, 0.86f, 0.45f);

            var barGo = new GameObject("TreeBar");
            barGo.transform.SetParent(_playRoot.transform, false);
            var barRt = barGo.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.5f, 1f);
            barRt.anchorMax = new Vector2(0.5f, 1f);
            barRt.pivot = new Vector2(0.5f, 1f);
            barRt.anchoredPosition = new Vector2(0f, -24f);
            barRt.sizeDelta = new Vector2(520f, 30f);
            var barBg = barGo.AddComponent<Image>();
            barBg.sprite = WhiteSprite();
            barBg.color = new Color(0f, 0f, 0f, 0.55f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = new Vector2(3f, 3f);
            fillRt.offsetMax = new Vector2(-3f, -3f);
            _treeFill = fillGo.AddComponent<Image>();
            _treeFill.sprite = WhiteSprite();
            _treeFill.type = Image.Type.Filled;
            _treeFill.fillMethod = Image.FillMethod.Horizontal;
            _treeFill.color = new Color(0.35f, 0.85f, 0.4f);

            _tree = Label(_playRoot.transform, "TREE", new Vector2(0f, -58f), new Vector2(0.5f, 1f), TextAnchor.UpperCenter, 22);
            _tree.rectTransform.sizeDelta = new Vector2(760f, 32f);
            _objective = Label(_playRoot.transform, "OBJ", new Vector2(0f, -88f), new Vector2(0.5f, 1f), TextAnchor.UpperCenter, 20);
            _objective.rectTransform.sizeDelta = new Vector2(900f, 32f);
            _objective.color = new Color(1f, 0.92f, 0.55f);
            _objective.text = "PROTECT THE TREE  —  if this bar hits 0, you lose";

            _crosshair = Label(_playRoot.transform, "+", Vector2.zero, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 36);
            _crosshair.color = new Color(1f, 1f, 1f, 0.85f);
            _crosshairScale = _crosshair.rectTransform.localScale;

            _coach = Label(_playRoot.transform, "Coach", new Vector2(0f, 118f), new Vector2(0.5f, 0f), TextAnchor.LowerCenter, 30);
            _coach.rectTransform.sizeDelta = new Vector2(1500f, 70f);
            _coach.color = new Color(1f, 0.9f, 0.45f);
            _coach.text = "";

            _hint = Label(_playRoot.transform, "Hint", new Vector2(0f, 18f), new Vector2(0.5f, 0f), TextAnchor.LowerCenter, 20);
            _hint.rectTransform.sizeDelta = new Vector2(1700f, 44f);
            _hint.color = new Color(0.95f, 0.97f, 1f, 0.92f);
            _hint.text = "WASD move   ·   Mouse look   ·   Click throw   ·   Hold click to charge   ·   F kick   ·   Esc pause";

            _chargeRoot = Bar(_playRoot.transform, "Charge", new Vector2(0f, -48f), new Vector2(0.5f, 0.5f), new Vector2(220f, 14f), out _chargeFill);
            _chargeFill.color = new Color(0.7f, 0.92f, 1f);
            _chargeLabel = Label(_chargeRoot.transform, "ChargeLbl", new Vector2(0f, 16f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 16);
            _chargeLabel.rectTransform.sizeDelta = new Vector2(280f, 24f);
            _chargeLabel.text = "HOLD  —  release to throw";

            var hitGo = new GameObject("HitMarker");
            hitGo.transform.SetParent(_playRoot.transform, false);
            var hitRt = hitGo.AddComponent<RectTransform>();
            hitRt.anchorMin = hitRt.anchorMax = hitRt.pivot = new Vector2(0.5f, 0.5f);
            hitRt.sizeDelta = new Vector2(48f, 48f);
            _hit = hitGo.AddComponent<Image>();
            _hit.sprite = WhiteSprite();
            _hit.color = new Color(1f, 1f, 1f, 0f);
            _hit.enabled = false;

            _banner = Label(_playRoot.transform, "Banner", Vector2.zero, new Vector2(0.5f, 0.62f), TextAnchor.MiddleCenter, 58);
            _banner.rectTransform.sizeDelta = new Vector2(1500f, 90f);
            _banner.gameObject.SetActive(false);

            var flashGo = new GameObject("Flash");
            flashGo.transform.SetParent(transform, false);
            var flashRt = flashGo.AddComponent<RectTransform>();
            Stretch(flashRt);
            _flash = flashGo.AddComponent<Image>();
            _flash.sprite = WhiteSprite();
            _flash.color = new Color(0.7f, 0.05f, 0.05f, 0f);
            _flash.raycastTarget = false;

            BuildTitle();
            BuildPause();
            BuildGameOver();
            _playRoot.SetActive(false);
        }

        void BuildTitle()
        {
            _titleRoot = Panel("Title");
            ChildText(_titleRoot.transform, "GNOME GUARD", new Vector2(0f, 280f), 86);
            var tag = ChildText(_titleRoot.transform, "PROTECT THE CHRISTMAS TREE", new Vector2(0f, 200f), 28);
            tag.color = new Color(1f, 0.85f, 0.4f);

            var how = ChildText(_titleRoot.transform,
                "Green zombie gnomes will try to eat the tree.\nThrow snowballs at them. If the tree's health hits 0, you lose.",
                new Vector2(0f, 130f), 22);
            how.rectTransform.sizeDelta = new Vector2(1400f, 70f);
            how.color = new Color(0.92f, 0.95f, 1f);

            string[] keys = { "WASD", "MOUSE", "CLICK", "HOLD", "F", "SHIFT" };
            string[] acts = { "Move", "Look", "Throw", "Charge throw", "Kick", "Sprint" };
            float startX = -575f;
            for (int i = 0; i < keys.Length; i++)
                ControlCard(_titleRoot.transform, new Vector2(startX + i * 230f, -20f), keys[i], acts[i]);

            var start = ChildText(_titleRoot.transform, "CLICK  or  press  SPACE  to start", new Vector2(0f, -210f), 34);
            start.color = new Color(0.55f, 1f, 0.65f);
            var note = ChildText(_titleRoot.transform, "Click the Game view first so Unity captures the mouse.", new Vector2(0f, -265f), 20);
            note.color = new Color(1f, 1f, 1f, 0.7f);
            note.rectTransform.sizeDelta = new Vector2(1200f, 36f);
        }

        void BuildPause()
        {
            _pauseRoot = Panel("Pause");
            ChildText(_pauseRoot.transform, "PAUSED", new Vector2(0f, 200f), 72);
            ChildText(_pauseRoot.transform, "Esc  to resume", new Vector2(0f, 130f), 26);
            string[] keys = { "WASD", "MOUSE", "CLICK", "HOLD", "F", "ESC" };
            string[] acts = { "Move", "Look", "Throw", "Charge", "Kick", "Resume" };
            float startX = -575f;
            for (int i = 0; i < keys.Length; i++)
                ControlCard(_pauseRoot.transform, new Vector2(startX + i * 230f, -20f), keys[i], acts[i]);
            var tip = ChildText(_pauseRoot.transform, "Keep the green bar full. Walk into glowing holiday gnomes for power-ups.", new Vector2(0f, -200f), 22);
            tip.rectTransform.sizeDelta = new Vector2(1400f, 50f);
            _pauseRoot.SetActive(false);
        }

        void BuildGameOver()
        {
            _overRoot = Panel("GameOver");
            ChildText(_overRoot.transform, "THE TREE FELL", new Vector2(0f, 160f), 70);
            var why = ChildText(_overRoot.transform, "Zombie gnomes reached the Christmas tree.", new Vector2(0f, 90f), 24);
            why.color = new Color(1f, 0.85f, 0.55f);
            _overBody = ChildText(_overRoot.transform, "", new Vector2(0f, 10f), 30);
            _overBody.rectTransform.sizeDelta = new Vector2(1100f, 160f);
            MakeButton(_overRoot.transform, "RESTART", new Vector2(0f, -150f), () =>
            {
                if (GnomeGuardGame.Instance != null)
                    GnomeGuardGame.Instance.RequestRestart();
            });
            var again = ChildText(_overRoot.transform, "or press  R", new Vector2(0f, -230f), 24);
            again.color = new Color(0.55f, 1f, 0.65f);
            _overRoot.SetActive(false);
        }

        void Update()
        {
            if (_banner != null && _banner.gameObject.activeSelf && Time.unscaledTime > _bannerUntil)
                _banner.gameObject.SetActive(false);

            if (_flash != null)
            {
                var c = _flash.color;
                c.a = Time.unscaledTime < _flashUntil ? Mathf.MoveTowards(c.a, 0.28f, Time.unscaledDeltaTime * 6f) : Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 2.4f);
                _flash.color = c;
            }

            if (_crosshair != null)
            {
                float punch = Time.unscaledTime < _hitUntil ? 1.35f : 1f;
                _crosshair.rectTransform.localScale = Vector3.Lerp(_crosshair.rectTransform.localScale, _crosshairScale * punch, Time.unscaledDeltaTime * 14f);
            }
        }

        public void SetPlayingHud(bool playing)
        {
            if (_playRoot != null) _playRoot.SetActive(playing);
            if (!playing && _chargeRoot != null) _chargeRoot.SetActive(false);
            ShowPause(false);
        }

        public void ShowTitle(bool show) => _titleRoot.SetActive(show);

        public void ShowPause(bool show)
        {
            if (_pauseRoot != null) _pauseRoot.SetActive(show);
        }

        public void ShowGameOver(int wave, int score, int best)
        {
            _overRoot.SetActive(true);
            _overBody.text = $"You reached wave {wave}\nScore  {score}     Best  {best}";
        }

        public void HideGameOver() => _overRoot.SetActive(false);

        public void SetCoach(string text)
        {
            if (_coach == null) return;
            _coach.text = text ?? "";
        }

        public void SetStats(int score, int combo, int wave, int remaining, int hp, int maxHp, string status, float charge)
        {
            _score.text = combo > 1 ? $"SCORE  {score}   x{combo} COMBO" : $"SCORE  {score}";
            _score.color = combo >= 8 ? new Color(1f, 0.82f, 0.28f) : combo >= 4 ? new Color(1f, 0.92f, 0.65f) : Color.white;
            _wave.text = remaining > 0 ? $"WAVE  {wave}    {remaining} gnomes left" : $"WAVE  {wave}";
            _tree.text = $"CHRISTMAS TREE   {Mathf.Max(0, hp)} / {maxHp}";
            _treeFill.fillAmount = maxHp <= 0 ? 0f : Mathf.Clamp01(hp / (float)maxHp);
            _treeFill.color = Color.Lerp(new Color(0.85f, 0.2f, 0.15f), new Color(0.35f, 0.85f, 0.4f), _treeFill.fillAmount);
            _status.text = string.IsNullOrEmpty(status) ? "" : "POWER  " + status;
            if (_chargeFill != null) _chargeFill.fillAmount = charge;
            if (_chargeRoot != null) _chargeRoot.SetActive(charge > 0.02f);
        }

        public void Banner(string text, float seconds)
        {
            _banner.text = text;
            _banner.gameObject.SetActive(true);
            _bannerUntil = Time.unscaledTime + seconds;
        }

        public void DamageFlash()
        {
            _flashUntil = Time.unscaledTime + 0.18f;
        }

        public void HitMarker()
        {
            _hitUntil = Time.unscaledTime + 0.09f;
        }

        void ControlCard(Transform parent, Vector2 pos, string key, string action)
        {
            var go = new GameObject("Card_" + key);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(210f, 110f);
            var img = go.AddComponent<Image>();
            img.sprite = WhiteSprite();
            img.color = new Color(0f, 0f, 0f, 0.5f);
            img.raycastTarget = false;

            var k = ChildText(go.transform, key, new Vector2(0f, 18f), 26);
            k.color = new Color(1f, 0.86f, 0.4f);
            k.rectTransform.sizeDelta = new Vector2(200f, 40f);
            var a = ChildText(go.transform, action, new Vector2(0f, -22f), 20);
            a.rectTransform.sizeDelta = new Vector2(200f, 36f);
        }

        GameObject Bar(Transform parent, string name, Vector2 pos, Vector2 anchor, Vector2 size, out Image fill)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var bg = go.AddComponent<Image>();
            bg.sprite = WhiteSprite();
            bg.color = new Color(0f, 0f, 0f, 0.45f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(go.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);
            fill = fillGo.AddComponent<Image>();
            fill.sprite = WhiteSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.color = Color.white;
            go.SetActive(false);
            return go;
        }

        Text Label(Transform parent, string name, Vector2 pos, Vector2 anchor, TextAnchor align, int size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(900f, 56f);
            var text = go.AddComponent<Text>();
            Style(text, size, align);
            return text;
        }

        void MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(360f, 78f);

            var img = go.AddComponent<Image>();
            img.sprite = WhiteSprite();
            img.color = new Color(0.16f, 0.58f, 0.28f, 0.95f);
            img.raycastTarget = true;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 1f, 0.88f);
            colors.pressedColor = new Color(0.7f, 0.85f, 0.72f);
            colors.selectedColor = colors.highlightedColor;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var text = ChildText(go.transform, label, Vector2.zero, 36);
            text.rectTransform.sizeDelta = new Vector2(340f, 70f);
            text.raycastTarget = false;
        }

        GameObject Panel(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            Stretch(rt);
            var img = go.AddComponent<Image>();
            img.sprite = WhiteSprite();
            img.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
            img.raycastTarget = name == "GameOver";
            return go;
        }

        Text ChildText(Transform parent, string value, Vector2 pos, int size)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(1500f, 120f);
            var text = go.AddComponent<Text>();
            Style(text, size, TextAnchor.MiddleCenter);
            text.text = value;
            return text;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void Style(Text text, int size, TextAnchor align)
        {
            text.font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Verdana" }, size);
            if (text.font == null)
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = align;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.6f, -1.6f);
        }

        static Sprite WhiteSprite()
        {
            var tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
