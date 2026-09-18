using UnityEngine;
using UnityEngine.UI;

namespace GnomeGuard
{
    public class GameHud : MonoBehaviour
    {
        Text _score;
        Text _wave;
        Text _tree;
        Text _status;
        Text _banner;
        Text _titleSub;
        Text _crosshair;
        Text _hint;
        Image _treeFill;
        Image _flash;
        Image _chargeFill;
        Image _hit;
        GameObject _titleRoot;
        GameObject _overRoot;
        GameObject _chargeRoot;
        Text _overBody;
        float _bannerUntil;
        float _flashUntil;
        float _hitUntil;
        Vector3 _crosshairScale = Vector3.one;

        public static GameHud Create(Transform parent)
        {
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

        void Build()
        {
            _score = Label("Score", new Vector2(48f, -36f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 42);
            _wave = Label("Wave", new Vector2(48f, -88f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 28);
            _status = Label("Status", new Vector2(48f, -132f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 24);
            _status.color = new Color(1f, 0.86f, 0.45f);

            var barGo = new GameObject("TreeBar");
            barGo.transform.SetParent(transform, false);
            var barRt = barGo.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.5f, 1f);
            barRt.anchorMax = new Vector2(0.5f, 1f);
            barRt.pivot = new Vector2(0.5f, 1f);
            barRt.anchoredPosition = new Vector2(0f, -28f);
            barRt.sizeDelta = new Vector2(420f, 28f);
            var barBg = barGo.AddComponent<Image>();
            barBg.sprite = WhiteSprite();
            barBg.color = new Color(0f, 0f, 0f, 0.45f);

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

            _tree = Label("TREE", new Vector2(0f, -62f), new Vector2(0.5f, 1f), TextAnchor.UpperCenter, 22);
            _tree.rectTransform.sizeDelta = new Vector2(500f, 36f);

            _crosshair = Label("+", Vector2.zero, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 36);
            _crosshair.color = new Color(1f, 1f, 1f, 0.85f);
            _crosshairScale = _crosshair.rectTransform.localScale;

            _hint = Label("Hint", new Vector2(0f, 48f), new Vector2(0.5f, 0f), TextAnchor.LowerCenter, 20);
            _hint.rectTransform.sizeDelta = new Vector2(1400f, 40f);
            _hint.color = new Color(1f, 1f, 1f, 0.7f);
            _hint.text = "Hold click to charge   F kick   Shift sprint";

            _chargeRoot = Bar("Charge", new Vector2(0f, -42f), new Vector2(0.5f, 0.5f), new Vector2(160f, 10f), out _chargeFill);
            _chargeFill.color = new Color(0.7f, 0.92f, 1f);

            var hitGo = new GameObject("HitMarker");
            hitGo.transform.SetParent(transform, false);
            var hitRt = hitGo.AddComponent<RectTransform>();
            hitRt.anchorMin = hitRt.anchorMax = hitRt.pivot = new Vector2(0.5f, 0.5f);
            hitRt.sizeDelta = new Vector2(48f, 48f);
            _hit = hitGo.AddComponent<Image>();
            _hit.sprite = WhiteSprite();
            _hit.color = new Color(1f, 1f, 1f, 0f);
            _hit.enabled = false;
            hitRt.localScale = new Vector3(0.22f, 0.04f, 1f);

            _banner = Label("", Vector2.zero, new Vector2(0.5f, 0.58f), TextAnchor.MiddleCenter, 64);
            _banner.rectTransform.sizeDelta = new Vector2(1400f, 90f);
            _banner.gameObject.SetActive(false);

            var flashGo = new GameObject("Flash");
            flashGo.transform.SetParent(transform, false);
            var flashRt = flashGo.AddComponent<RectTransform>();
            flashRt.anchorMin = Vector2.zero;
            flashRt.anchorMax = Vector2.one;
            flashRt.offsetMin = Vector2.zero;
            flashRt.offsetMax = Vector2.zero;
            _flash = flashGo.AddComponent<Image>();
            _flash.sprite = WhiteSprite();
            _flash.color = new Color(0.7f, 0.05f, 0.05f, 0f);
            _flash.raycastTarget = false;

            _titleRoot = Panel("Title");
            ChildText(_titleRoot.transform, "GNOME GUARD", new Vector2(0f, 110f), 92);
            _titleSub = ChildText(_titleRoot.transform,
                "Protect the Christmas tree from zombie gnomes.\nWASD move   Mouse look   Click / hold to throw\nF kick   Shift sprint   Collect holiday gnomes\n\nClick the Game view, then click or press SPACE.",
                new Vector2(0f, -70f), 26);
            _titleSub.rectTransform.sizeDelta = new Vector2(1500f, 280f);

            _overRoot = Panel("GameOver");
            ChildText(_overRoot.transform, "THE TREE FELL", new Vector2(0f, 80f), 72);
            _overBody = ChildText(_overRoot.transform, "", new Vector2(0f, -40f), 32);
            _overBody.rectTransform.sizeDelta = new Vector2(1100f, 220f);
            _overRoot.SetActive(false);
        }

        void Update()
        {
            if (_banner.gameObject.activeSelf && Time.unscaledTime > _bannerUntil)
                _banner.gameObject.SetActive(false);

            if (_flash != null)
            {
                var c = _flash.color;
                c.a = Time.unscaledTime < _flashUntil ? Mathf.MoveTowards(c.a, 0.28f, Time.unscaledDeltaTime * 6f) : Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 2.4f);
                _flash.color = c;
            }

            if (_hit != null && _hit.enabled)
            {
                var c = _hit.color;
                c.a = Time.unscaledTime < _hitUntil ? 0.9f : Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 6f);
                _hit.color = c;
            }

            if (_crosshair != null)
            {
                float punch = Time.unscaledTime < _hitUntil ? 1.35f : 1f;
                _crosshair.rectTransform.localScale = Vector3.Lerp(_crosshair.rectTransform.localScale, _crosshairScale * punch, Time.unscaledDeltaTime * 14f);
            }
        }

        public void SetPlayingHud(bool playing)
        {
            _crosshair.gameObject.SetActive(playing);
            _score.gameObject.SetActive(playing);
            _wave.gameObject.SetActive(playing);
            _status.gameObject.SetActive(playing);
            _hint.gameObject.SetActive(playing);
            if (!playing && _chargeRoot != null) _chargeRoot.SetActive(false);
        }

        public void ShowTitle(bool show) => _titleRoot.SetActive(show);

        public void ShowGameOver(int wave, int score, int best)
        {
            _overRoot.SetActive(true);
            _overBody.text = $"Wave {wave}   Score {score}\nBest {best}\n\nPress R to restart";
        }

        public void HideGameOver() => _overRoot.SetActive(false);

        public void SetStats(int score, int combo, int wave, int remaining, int hp, int maxHp, string status, float charge)
        {
            _score.text = combo > 1 ? $"SCORE  {score}   x{combo} COMBO" : $"SCORE  {score}";
            _score.color = combo >= 8 ? new Color(1f, 0.82f, 0.28f) : combo >= 4 ? new Color(1f, 0.92f, 0.65f) : Color.white;
            _wave.text = remaining > 0 ? $"WAVE  {wave}    {remaining} left" : $"WAVE  {wave}";
            _tree.text = $"TREE  {Mathf.Max(0, hp)} / {maxHp}";
            _treeFill.fillAmount = maxHp <= 0 ? 0f : Mathf.Clamp01(hp / (float)maxHp);
            _treeFill.color = Color.Lerp(new Color(0.85f, 0.2f, 0.15f), new Color(0.35f, 0.85f, 0.4f), _treeFill.fillAmount);
            _status.text = string.IsNullOrEmpty(status) ? "" : status;
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

        GameObject Bar(string name, Vector2 pos, Vector2 anchor, Vector2 size, out Image fill)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var bg = go.AddComponent<Image>();
            bg.sprite = WhiteSprite();
            bg.color = new Color(0f, 0f, 0f, 0.4f);

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

        Text Label(string name, Vector2 pos, Vector2 anchor, TextAnchor align, int size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
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

        GameObject Panel(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite = WhiteSprite();
            img.color = new Color(0.02f, 0.04f, 0.08f, 0.62f);
            img.raycastTarget = false;
            return go;
        }

        Text ChildText(Transform parent, string value, Vector2 pos, int size)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(1500f, 120f);
            var text = go.AddComponent<Text>();
            Style(text, size, TextAnchor.MiddleCenter);
            text.text = value;
            return text;
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
