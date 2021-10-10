using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using MyBox;
using DG.Tweening;
using UnityEngine.UI;

public class TweenAnimation : MonoBehaviour
{
    /// <summary>
    /// 播放類型
    /// </summary>
    public enum PlayMode
    {
        Normal,
        Reverse,
        PingPong,
        Sequence
    }

    [Header("Options")]
    [SerializeField, Tooltip("勾選後各種類的Origin將會初始成以Begin為主")]
    private bool setOriginIsBeginValue = false;
    [SerializeField, Tooltip("【自動播放的不用勾選】勾選後將會在Start做一次初始")]
    private bool firstInit = false;
    [SerializeField, Tooltip("【啟用後將不會自動播放】勾選啟用只能透過invoke PlayTween(true or false)的方式執行播放")]
    private bool playByEvent = false;
    [SerializeField, Tooltip("勾選啟用時初始會先設置active = false, 將會於動畫播放完畢時自動設置對應的active狀態")]
    private bool autoActive = false;
    [SerializeField, Tooltip("勾選啟用會在OnEnable()執行reset步驟")]
    private bool resetOnEnable = false;
    [SerializeField, Tooltip("勾選啟用會在OnDisable()執行reset步驟")]
    private bool resetOnDisable = false;

    // --- tPosition, tRotation, tScale
    [Header("Transform")]
    [SerializeField, Tooltip("啟用Position動畫, 必須啟用才有效果")]
    private bool tPositionOn = false;
    [SerializeField, Tooltip("啟用Rotation動畫, 必須啟用才有效果")]
    private bool tRotationOn = false;
    [SerializeField, Tooltip("啟用Scale動畫, 必須啟用才有效果")]
    private bool tScaleOn = false;

    // --- tSize
    private RectTransform _rectTransform;
    [Header("RectTransform")]
    [SerializeField, Tooltip("【RectTransform - AutoFind】啟用RectTransform Size動畫, 必須啟用才有效果")]
    private bool tSizeOn = false;

    // --- tAlpha
    private CanvasGroup _cg;
    [Header("CanvasGroup")]
    [SerializeField, Tooltip("【CanvasGroup - AutoFind】啟用CanvasGroup Alpha動畫, 必須啟用才有效果")]
    private bool tAlphaOn = false;

    // --- tImageColor
    private Image _img;
    [Header("Image")]
    [SerializeField, Tooltip("【Image - AutoFind】啟用Image Color動畫, 必須啟用才有效果")]
    private bool tImgColorOn = false;

    // --- tSpriteColor
    private SpriteRenderer _spr;
    [Header("SpriteRenderer")]
    [SerializeField, Tooltip("【SpriteRenderer - AutoFind】啟用Sprite Color動畫, 必須啟用才有效果")]
    private bool tSprColorOn = false;

    #region TweenBase
    [Serializable]
    public class TweenBase
    {
        [SerializeField, Tooltip("間隔播放")]
        public bool isInterval = false;
        [SerializeField, ConditionalField(nameof(isInterval)), Tooltip("間隔播放時間")]
        public float intervalTime = 0.0f;
        [HideInInspector]
        public DeltaTimer intervalTimer = null;

        [SerializeField, ConditionalField(nameof(isInterval), inverse: true), Tooltip("循環播放, -1 = Infinitely")]
        public int loopTimes = 0;

        [SerializeField, Tooltip("Tween動畫的播放模式")]
        public PlayMode playMode = PlayMode.Normal;
        [SerializeField, SearchableEnum, Tooltip("Tween動畫的過渡模式")]
        public Ease easeMode = Ease.Linear;

        [SerializeField, Tooltip("持續時間")]
        public float duration = 0.1f;

        [HideInInspector]
        public TweenCallback callback = null;
        [HideInInspector]
        public bool autoActive = false;

        [HideInInspector]
        public DG.Tweening.Sequence seq = null;

        public TweenBase()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public virtual void Reset()
        {
            this.seq.Kill();
            this.intervalTimer.Pause();
            this.callback = null;
            this.autoActive = false;
        }

        public virtual void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false) { }

        public virtual void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false) { }

        public virtual void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false) { }

        public virtual void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false) { }

        public virtual void TickPlay() { }
    }
    #endregion

    #region TweenPosition
    [Serializable]
    public class TweenPosition : TweenBase
    {
        [HideInInspector]
        public Transform transform = null;

        [HideInInspector]
        public Vector3 originPosition = new Vector3(0, 0, 0);

        [SerializeField, Tooltip("based on own position to tween move")]
        public bool basedOnOwnPos = false;
        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector3 from = new Vector3(0, 0, 0);
        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector3 to = new Vector3(0, 0, 0);

        [Serializable]
        public class Sequence
        {
            public List<Vector3> sequence = new List<Vector3>();
        }
        [SerializeField, ConditionalField(nameof(playMode), false, PlayMode.Sequence)]
        public Sequence posSeq = new Sequence();

        public TweenPosition()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public void Init(Vector3 vec3)
        {
            this.originPosition = vec3;
        }
        public void Init()
        {
            Vector3 withOwnPos = Vector3.zero;

            switch (this.playMode)
            {
                case PlayMode.Reverse:
                    // 是否疊加自身的Postion
                    if (this.basedOnOwnPos) withOwnPos = new Vector3(this.transform.localPosition.x + this.to.x, this.transform.localPosition.y + this.to.y, this.transform.position.z + this.to.z);
                    this.originPosition = (this.basedOnOwnPos) ? withOwnPos : this.to;
                    break;
                case PlayMode.Sequence:
                    // 是否疊加自身的Postion
                    if (this.basedOnOwnPos) withOwnPos = new Vector3(this.transform.localPosition.x + this.posSeq.sequence[0].x, this.transform.localPosition.y + this.posSeq.sequence[0].y, this.transform.position.z + this.posSeq.sequence[0].z);
                    this.originPosition = (this.basedOnOwnPos) ? withOwnPos : this.posSeq.sequence[0];
                    break;
                default:
                    // 是否疊加自身的Postion
                    if (this.basedOnOwnPos) withOwnPos = new Vector3(this.transform.localPosition.x + this.from.x, this.transform.localPosition.y + this.from.y, this.transform.position.z + this.from.z);
                    this.originPosition = (this.basedOnOwnPos) ? withOwnPos : this.from;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            this.transform.localPosition = this.originPosition;
        }

        public override void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            Vector3 fromWithOwnPos = Vector3.zero;
            Vector3 toWithOwnPos = Vector3.zero;
            if (this.basedOnOwnPos)
            {
                fromWithOwnPos = new Vector3(this.transform.localPosition.x + this.from.x, this.transform.localPosition.y + this.from.y, this.transform.position.z + this.from.z);
                toWithOwnPos = new Vector3(this.transform.localPosition.x + this.to.x, this.transform.localPosition.y + this.to.y, this.transform.position.z + this.to.z);
            }

            if (!inverse)
            {
                this.transform.localPosition = (this.basedOnOwnPos) ? fromWithOwnPos : this.from;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? toWithOwnPos : this.to, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.localPosition = (this.basedOnOwnPos) ? toWithOwnPos : this.to;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? fromWithOwnPos : this.from, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }

        }

        public override void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            Vector3 fromWithOwnPos = Vector3.zero;
            Vector3 toWithOwnPos = Vector3.zero;
            if (this.basedOnOwnPos)
            {
                fromWithOwnPos = new Vector3(this.transform.localPosition.x + this.from.x, this.transform.localPosition.y + this.from.y, this.transform.position.z + this.from.z);
                toWithOwnPos = new Vector3(this.transform.localPosition.x + this.to.x, this.transform.localPosition.y + this.to.y, this.transform.position.z + this.to.z);
            }

            if (!inverse)
            {
                this.transform.localPosition = (this.basedOnOwnPos) ? toWithOwnPos : this.to;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? fromWithOwnPos : this.from, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.localPosition = (this.basedOnOwnPos) ? fromWithOwnPos : this.from;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? toWithOwnPos : this.to, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            Vector3 fromWithOwnPos = Vector3.zero;
            Vector3 toWithOwnPos = Vector3.zero;
            if (this.basedOnOwnPos)
            {
                fromWithOwnPos = new Vector3(this.transform.localPosition.x + this.from.x, this.transform.localPosition.y + this.from.y, this.transform.position.z + this.from.z);
                toWithOwnPos = new Vector3(this.transform.localPosition.x + this.to.x, this.transform.localPosition.y + this.to.y, this.transform.position.z + this.to.z);
            }

            if (!inverse)
            {
                this.transform.localPosition = (this.basedOnOwnPos) ? fromWithOwnPos : this.from;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? toWithOwnPos : this.to, this.duration / 2));
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? fromWithOwnPos : this.from, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.localPosition = (this.basedOnOwnPos) ? toWithOwnPos : this.to;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? fromWithOwnPos : this.from, this.duration / 2));
                this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? toWithOwnPos : this.to, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.posSeq.sequence.Count == 0) return;

            if (!inverse)
            {
                Vector3 withOwnPos = Vector3.zero;
                if (this.basedOnOwnPos) withOwnPos = new Vector3(this.transform.localPosition.x + this.posSeq.sequence[0].x, this.transform.localPosition.y + this.posSeq.sequence[0].y, this.transform.localPosition.z + this.posSeq.sequence[0].z);
                this.transform.localPosition = (this.basedOnOwnPos) ? withOwnPos : this.posSeq.sequence[0];

                int count = this.posSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                this.posSeq.sequence.ForEach(pos =>
                {
                    if (this.basedOnOwnPos) withOwnPos = new Vector3(this.transform.localPosition.x + pos.x, this.transform.localPosition.y + pos.y, this.transform.localPosition.z + pos.z);
                    this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? withOwnPos : pos, this.duration / count));
                });
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                Vector3 withOwnPos = Vector3.zero;
                if (this.basedOnOwnPos) withOwnPos = new Vector3(this.transform.localPosition.x + this.posSeq.sequence[this.posSeq.sequence.Count - 1].x, this.transform.localPosition.y + this.posSeq.sequence[this.posSeq.sequence.Count - 1].y, this.transform.localPosition.z + this.posSeq.sequence[this.posSeq.sequence.Count - 1].z);
                this.transform.localPosition = (this.basedOnOwnPos) ? withOwnPos : this.posSeq.sequence[this.posSeq.sequence.Count - 1];

                int count = this.posSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                for (int i = (this.posSeq.sequence.Count - 1); i >= 0; i--)
                {
                    if (this.basedOnOwnPos) withOwnPos = new Vector3(this.transform.localPosition.x + this.posSeq.sequence[i].x, this.transform.localPosition.y + this.posSeq.sequence[i].y, this.transform.localPosition.z + this.posSeq.sequence[i].z);
                    this.seq.Append(this.transform.DOLocalMove((this.basedOnOwnPos) ? withOwnPos : this.posSeq.sequence[i], this.duration / count));
                }
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void TickPlay()
        {
            switch (this.playMode)
            {
                case PlayMode.Normal:
                    this.DoTweenNormal();
                    break;
                case PlayMode.Reverse:
                    this.DoTweenReverse();
                    break;
                case PlayMode.PingPong:
                    this.DoTweenPingPong();
                    break;
                case PlayMode.Sequence:
                    this.DoTweenSequence();
                    break;
            }
        }
    }
    #endregion
    [Header("TweenValues")]
    [SerializeField, ConditionalField(nameof(tPositionOn))]
    private TweenPosition tPosition = new TweenPosition();

    #region TweenRotation
    [Serializable]
    public class TweenRotation : TweenBase
    {
        [HideInInspector]
        public Transform transform = null;

        [HideInInspector]
        public Vector3 originAngle = new Vector3();

        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector3 beginAngle = new Vector3();
        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector3 endAngle = new Vector3();

        [Serializable]
        public class Sequence
        {
            public List<Vector3> sequence = new List<Vector3>();
        }
        [SerializeField, ConditionalField(nameof(playMode), false, PlayMode.Sequence)]
        public Sequence angleSeq = new Sequence();

        public TweenRotation()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public void Init(Vector3 angle)
        {
            this.originAngle = angle;
        }
        public void Init()
        {
            switch (this.playMode)
            {
                case PlayMode.Reverse:
                    this.originAngle = this.endAngle;
                    break;
                case PlayMode.Sequence:
                    this.originAngle = this.angleSeq.sequence[0];
                    break;
                default:
                    this.originAngle = this.beginAngle;
                    break;
            }
        }

        private Quaternion _ConvertToEuler(Vector3 vec3)
        {
            return Quaternion.Euler(vec3.x, vec3.y, vec3.z);
        }

        public override void Reset()
        {
            base.Reset();
            this.transform.rotation = this._ConvertToEuler(this.originAngle);
        }

        public override void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (!inverse)
            {
                this.transform.rotation = this._ConvertToEuler(this.beginAngle);

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalRotate(this.endAngle, this.duration, RotateMode.FastBeyond360));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.rotation = this._ConvertToEuler(this.endAngle);

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalRotate(this.beginAngle, this.duration, RotateMode.FastBeyond360));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (!inverse)
            {
                this.transform.rotation = this._ConvertToEuler(this.endAngle);

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalRotate(this.beginAngle, this.duration, RotateMode.FastBeyond360));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.rotation = this._ConvertToEuler(this.beginAngle);

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalRotate(this.endAngle, this.duration, RotateMode.FastBeyond360));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (!inverse)
            {
                this.transform.rotation = this._ConvertToEuler(this.beginAngle);

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalRotateQuaternion(this._ConvertToEuler(this.endAngle), this.duration / 2));
                this.seq.Append(this.transform.DOLocalRotateQuaternion(this._ConvertToEuler(this.beginAngle), this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.rotation = this._ConvertToEuler(this.endAngle);

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOLocalRotateQuaternion(this._ConvertToEuler(this.beginAngle), this.duration / 2));
                this.seq.Append(this.transform.DOLocalRotateQuaternion(this._ConvertToEuler(this.endAngle), this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.angleSeq.sequence.Count == 0) return;

            if (!inverse)
            {
                this.transform.rotation = this._ConvertToEuler(this.angleSeq.sequence[0]);

                int count = this.angleSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                this.angleSeq.sequence.ForEach(angle =>
                {
                    this.seq.Append(this.transform.DOLocalRotateQuaternion(this._ConvertToEuler(angle), this.duration / count));
                });
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.rotation = this._ConvertToEuler(this.angleSeq.sequence[this.angleSeq.sequence.Count - 1]);

                int count = this.angleSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                for (int i = (this.angleSeq.sequence.Count - 1); i >= 0; i--)
                {
                    this.seq.Append(this.transform.DOLocalRotateQuaternion(this._ConvertToEuler(this.angleSeq.sequence[i]), this.duration / count));
                }
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void TickPlay()
        {
            switch (this.playMode)
            {
                case PlayMode.Normal:
                    this.DoTweenNormal();
                    break;
                case PlayMode.Reverse:
                    this.DoTweenReverse();
                    break;
                case PlayMode.PingPong:
                    this.DoTweenPingPong();
                    break;
                case PlayMode.Sequence:
                    this.DoTweenSequence();
                    break;
            }
        }
    }
    #endregion
    [SerializeField, ConditionalField(nameof(tRotationOn))]
    private TweenRotation tRotation = new TweenRotation();

    #region TweenScale
    [Serializable]
    public class TweenScale : TweenBase
    {
        [HideInInspector]
        public Transform transform = null;

        [HideInInspector]
        public Vector3 originScale = new Vector3(0, 0, 0);

        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector3 beginScale = new Vector3(0, 0, 0);
        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector3 endScale = new Vector3(0, 0, 0);

        [Serializable]
        public class Sequence
        {
            public List<Vector3> sequence = new List<Vector3>();
        }
        [SerializeField, ConditionalField(nameof(playMode), false, PlayMode.Sequence)]
        public Sequence scaleSeq = new Sequence();

        public TweenScale()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public void Init(Vector3 scale)
        {
            this.originScale = scale;
        }
        public void Init()
        {
            switch (this.playMode)
            {
                case PlayMode.Reverse:
                    this.originScale = this.endScale;
                    break;
                case PlayMode.Sequence:
                    this.originScale = this.scaleSeq.sequence[0];
                    break;
                default:
                    this.originScale = this.beginScale;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            this.transform.localScale = this.originScale;
        }

        public override void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (!inverse)
            {
                this.transform.localScale = this.beginScale;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOScale(this.endScale, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.localScale = this.endScale;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOScale(this.beginScale, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (!inverse)
            {
                this.transform.localScale = this.endScale;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOScale(this.beginScale, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.localScale = this.beginScale;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOScale(this.endScale, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (!inverse)
            {
                this.transform.localScale = this.beginScale;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOScale(this.endScale, this.duration / 2));
                this.seq.Append(this.transform.DOScale(this.beginScale, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.localScale = this.endScale;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.transform.DOScale(this.beginScale, this.duration / 2));
                this.seq.Append(this.transform.DOScale(this.endScale, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.scaleSeq.sequence.Count == 0) return;

            if (!inverse)
            {
                this.transform.localScale = this.scaleSeq.sequence[0];

                int count = this.scaleSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                this.scaleSeq.sequence.ForEach(scale =>
                {
                    this.seq.Append(this.transform.DOScale(scale, this.duration / count));
                });
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.transform.localScale = this.scaleSeq.sequence[this.scaleSeq.sequence.Count - 1];

                int count = this.scaleSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                for (int i = (this.scaleSeq.sequence.Count - 1); i >= 0; i--)
                {
                    this.seq.Append(this.transform.DOScale(this.scaleSeq.sequence[i], this.duration / count));
                }
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.transform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void TickPlay()
        {
            switch (this.playMode)
            {
                case PlayMode.Normal:
                    this.DoTweenNormal();
                    break;
                case PlayMode.Reverse:
                    this.DoTweenReverse();
                    break;
                case PlayMode.PingPong:
                    this.DoTweenPingPong();
                    break;
                case PlayMode.Sequence:
                    this.DoTweenSequence();
                    break;
            }
        }
    }
    #endregion
    [SerializeField, ConditionalField(nameof(tScaleOn))]
    private TweenScale tScale = new TweenScale();

    #region TweenSize (RectTransform)
    [Serializable]
    public class TweenSize : TweenBase
    {
        [HideInInspector]
        public RectTransform rectTransform = null;

        [HideInInspector]
        public Vector2 originSize = new Vector2(0, 0);

        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector2 beginSize = new Vector2(0, 0);
        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Vector2 endSize = new Vector2(0, 0);

        [Serializable]
        public class Sequence
        {
            public List<Vector2> sequence = new List<Vector2>();
        }
        [SerializeField, ConditionalField(nameof(playMode), false, PlayMode.Sequence)]
        public Sequence sizeSeq = new Sequence();

        public TweenSize()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public void Init(Vector2 size)
        {
            this.originSize = size;
        }
        public void Init()
        {
            switch (this.playMode)
            {
                case PlayMode.Reverse:
                    this.originSize = this.endSize;
                    break;
                case PlayMode.Sequence:
                    this.originSize = this.sizeSeq.sequence[0];
                    break;
                default:
                    this.originSize = this.beginSize;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (this.rectTransform != null) this.rectTransform.sizeDelta = this.originSize;
        }

        public override void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.rectTransform == null) return;

            if (!inverse)
            {
                this.rectTransform.sizeDelta = this.beginSize;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.rectTransform.DOSizeDelta(this.endSize, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.rectTransform.sizeDelta = this.endSize;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.rectTransform.DOSizeDelta(this.beginSize, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.rectTransform == null) return;

            if (!inverse)
            {
                this.rectTransform.sizeDelta = this.endSize;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.rectTransform.DOSizeDelta(this.beginSize, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.rectTransform.sizeDelta = this.beginSize;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.rectTransform.DOSizeDelta(this.endSize, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.rectTransform == null) return;

            if (!inverse)
            {
                this.rectTransform.sizeDelta = this.beginSize;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.rectTransform.DOSizeDelta(this.endSize, this.duration / 2));
                this.seq.Append(this.rectTransform.DOSizeDelta(this.beginSize, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.rectTransform.sizeDelta = this.endSize;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.rectTransform.DOSizeDelta(this.beginSize, this.duration / 2));
                this.seq.Append(this.rectTransform.DOSizeDelta(this.endSize, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.rectTransform == null) return;

            if (this.sizeSeq.sequence.Count == 0) return;

            if (!inverse)
            {
                this.rectTransform.sizeDelta = this.sizeSeq.sequence[0];

                int count = this.sizeSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                this.sizeSeq.sequence.ForEach(size =>
                {
                    this.seq.Append(this.rectTransform.DOSizeDelta(size, this.duration / count));
                });
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.rectTransform.sizeDelta = this.sizeSeq.sequence[this.sizeSeq.sequence.Count - 1];

                int count = this.sizeSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                for (int i = (this.sizeSeq.sequence.Count - 1); i >= 0; i--)
                {
                    this.seq.Append(this.rectTransform.DOSizeDelta(this.sizeSeq.sequence[i], this.duration / count));
                }
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.rectTransform.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void TickPlay()
        {
            if (this.rectTransform == null) return;

            switch (this.playMode)
            {
                case PlayMode.Normal:
                    this.DoTweenNormal();
                    break;
                case PlayMode.Reverse:
                    this.DoTweenReverse();
                    break;
                case PlayMode.PingPong:
                    this.DoTweenPingPong();
                    break;
                case PlayMode.Sequence:
                    this.DoTweenSequence();
                    break;
            }
        }
    }
    #endregion
    [SerializeField, ConditionalField(nameof(tSizeOn))]
    private TweenSize tSize = new TweenSize();

    #region TweenAlpha (CanvasGroup)
    [Serializable]
    public class TweenAlpha : TweenBase
    {
        [HideInInspector]
        public CanvasGroup cg = null;

        [HideInInspector]
        public float originAlpha = 0.0f;

        [SerializeField, Range(0, 1), ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public float beginAlpha = 0.0f;
        [SerializeField, Range(0, 1), ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public float endAlpha = 0.0f;

        [Serializable]
        public class Sequence
        {
            public List<float> sequence = new List<float>();
        }
        [SerializeField, ConditionalField(nameof(playMode), false, PlayMode.Sequence)]
        public Sequence alphaSeq = new Sequence();

        public TweenAlpha()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public void Init(float alpha)
        {
            this.originAlpha = alpha;
        }
        public void Init()
        {
            switch (this.playMode)
            {
                case PlayMode.Reverse:
                    this.originAlpha = this.endAlpha;
                    break;
                case PlayMode.Sequence:
                    this.originAlpha = this.alphaSeq.sequence[0];
                    break;
                default:
                    this.originAlpha = this.beginAlpha;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (this.cg != null) this.cg.alpha = this.originAlpha;
        }

        public override void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.cg == null) return;

            if (!inverse)
            {
                this.cg.alpha = this.beginAlpha;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.cg.DOFade(this.endAlpha, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.cg.alpha = this.endAlpha;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.cg.DOFade(this.beginAlpha, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.cg == null) return;

            if (!inverse)
            {
                this.cg.alpha = this.endAlpha;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.cg.DOFade(this.beginAlpha, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.cg.alpha = this.beginAlpha;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.cg.DOFade(this.endAlpha, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.cg == null) return;

            if (!inverse)
            {
                this.cg.alpha = this.beginAlpha;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.cg.DOFade(this.endAlpha, this.duration / 2));
                this.seq.Append(this.cg.DOFade(this.beginAlpha, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.cg.alpha = this.endAlpha;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.cg.DOFade(this.beginAlpha, this.duration / 2));
                this.seq.Append(this.cg.DOFade(this.endAlpha, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.cg == null) return;

            if (this.alphaSeq.sequence.Count == 0) return;

            if (!inverse)
            {
                this.cg.alpha = this.alphaSeq.sequence[0];

                int count = this.alphaSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                this.alphaSeq.sequence.ForEach(alpha =>
                {
                    this.seq.Append(this.cg.DOFade(alpha, this.duration / count));
                });
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.cg.alpha = this.alphaSeq.sequence[this.alphaSeq.sequence.Count - 1];

                int count = this.alphaSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                for (int i = (this.alphaSeq.sequence.Count - 1); i >= 0; i--)
                {
                    this.seq.Append(this.cg.DOFade(this.alphaSeq.sequence[i], this.duration / count));
                }
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.cg.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void TickPlay()
        {
            if (this.cg == null) return;

            switch (this.playMode)
            {
                case PlayMode.Normal:
                    this.DoTweenNormal();
                    break;
                case PlayMode.Reverse:
                    this.DoTweenReverse();
                    break;
                case PlayMode.PingPong:
                    this.DoTweenPingPong();
                    break;
                case PlayMode.Sequence:
                    this.DoTweenSequence();
                    break;
            }
        }
    }
    #endregion
    [SerializeField, ConditionalField(nameof(tAlphaOn))]
    private TweenAlpha tAlpha = new TweenAlpha();

    #region TweenImageColor (Image)
    [Serializable]
    public class TweenImgColor : TweenBase
    {
        [HideInInspector]
        public Image img = null;

        [HideInInspector]
        public Color originColor = Color.white;

        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Color beginColor = Color.white;
        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Color endColor = Color.white;

        [Serializable]
        public class Sequence
        {
            public List<Color> sequence = new List<Color>();
        }
        [SerializeField, ConditionalField(nameof(playMode), false, PlayMode.Sequence)]
        public Sequence colorSeq = new Sequence();

        public TweenImgColor()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public void Init(Color color)
        {
            this.originColor = color;
        }
        public void Init()
        {
            switch (this.playMode)
            {
                case PlayMode.Reverse:
                    this.originColor = this.endColor;
                    break;
                case PlayMode.Sequence:
                    this.originColor = this.colorSeq.sequence[0];
                    break;
                default:
                    this.originColor = this.beginColor;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (this.img != null) this.img.color = this.originColor;
        }

        public override void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.img == null) return;

            if (!inverse)
            {
                this.img.color = this.beginColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.img.DOColor(this.endColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.img.color = this.endColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.img.DOColor(this.beginColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.img == null) return;

            if (!inverse)
            {
                this.img.color = this.endColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.img.DOColor(this.beginColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.img.color = this.beginColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.img.DOColor(this.endColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.img == null) return;

            if (!inverse)
            {
                this.img.color = this.beginColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.img.DOColor(this.endColor, this.duration / 2));
                this.seq.Append(this.img.DOColor(this.beginColor, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.img.color = this.endColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.img.DOColor(this.beginColor, this.duration / 2));
                this.seq.Append(this.img.DOColor(this.endColor, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.img == null) return;

            if (this.colorSeq.sequence.Count == 0) return;

            if (!inverse)
            {
                this.img.color = this.colorSeq.sequence[0];

                int count = this.colorSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                this.colorSeq.sequence.ForEach(color =>
                {
                    this.seq.Append(this.img.DOColor(color, this.duration / count));
                });
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.img.color = this.colorSeq.sequence[this.colorSeq.sequence.Count - 1];

                int count = this.colorSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                for (int i = (this.colorSeq.sequence.Count - 1); i >= 0; i--)
                {
                    this.seq.Append(this.img.DOColor(this.colorSeq.sequence[i], this.duration / count));
                }
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.img.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void TickPlay()
        {
            if (this.img == null) return;

            switch (this.playMode)
            {
                case PlayMode.Normal:
                    this.DoTweenNormal();
                    break;
                case PlayMode.Reverse:
                    this.DoTweenReverse();
                    break;
                case PlayMode.PingPong:
                    this.DoTweenPingPong();
                    break;
                case PlayMode.Sequence:
                    this.DoTweenSequence();
                    break;
            }
        }
    }
    #endregion
    [SerializeField, ConditionalField(nameof(tImgColorOn))]
    private TweenImgColor tImgColor = new TweenImgColor();

    #region TweenSprColor (Sprite)
    [Serializable]
    public class TweenSprColor : TweenBase
    {
        [HideInInspector]
        public SpriteRenderer spr = null;

        [HideInInspector]
        public Color originColor = Color.white;

        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Color beginColor = Color.white;
        [SerializeField, ConditionalField(nameof(playMode), inverse: true, PlayMode.Sequence)]
        public Color endColor = Color.white;

        [Serializable]
        public class Sequence
        {
            public List<Color> sequence = new List<Color>();
        }
        [SerializeField, ConditionalField(nameof(playMode), false, PlayMode.Sequence)]
        public Sequence colorSeq = new Sequence();

        public TweenSprColor()
        {
            this.intervalTimer = new DeltaTimer();
        }

        public void Init(Color color)
        {
            this.originColor = color;
        }
        public void Init()
        {
            switch (this.playMode)
            {
                case PlayMode.Reverse:
                    this.originColor = this.endColor;
                    break;
                case PlayMode.Sequence:
                    this.originColor = this.colorSeq.sequence[0];
                    break;
                default:
                    this.originColor = this.beginColor;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (this.spr != null) this.spr.color = this.originColor;
        }

        public override void DoTweenNormal(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.spr == null) return;

            if (!inverse)
            {
                this.spr.color = this.beginColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.spr.DOColor(this.endColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.spr.color = this.endColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.spr.DOColor(this.beginColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenReverse(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.spr == null) return;

            if (!inverse)
            {
                this.spr.color = this.endColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.spr.DOColor(this.beginColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.spr.color = this.beginColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.spr.DOColor(this.endColor, this.duration));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenPingPong(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.spr == null) return;

            if (!inverse)
            {
                this.spr.color = this.beginColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.spr.DOColor(this.endColor, this.duration / 2));
                this.seq.Append(this.spr.DOColor(this.beginColor, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.spr.color = this.endColor;

                this.seq = DG.Tweening.DOTween.Sequence();
                this.seq.Append(this.spr.DOColor(this.beginColor, this.duration / 2));
                this.seq.Append(this.spr.DOColor(this.endColor, this.duration / 2));
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void DoTweenSequence(int loopTimes = 0, bool inverse = false, bool trigger = false)
        {
            if (this.spr == null) return;

            if (this.colorSeq.sequence.Count == 0) return;

            if (!inverse)
            {
                this.spr.color = this.colorSeq.sequence[0];

                int count = this.colorSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                this.colorSeq.sequence.ForEach(color =>
                {
                    this.seq.Append(this.spr.DOColor(color, this.duration / count));
                });
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
            else
            {
                this.spr.color = this.colorSeq.sequence[this.colorSeq.sequence.Count - 1];

                int count = this.colorSeq.sequence.Count;
                this.seq = DG.Tweening.DOTween.Sequence();
                for (int i = (this.colorSeq.sequence.Count - 1); i >= 0; i--)
                {
                    this.seq.Append(this.spr.DOColor(this.colorSeq.sequence[i], this.duration / count));
                }
                if (this.callback != null) this.seq.AppendCallback(this.callback);
                if (!trigger && this.autoActive) this.seq.AppendCallback(() => { this.spr.gameObject.SetActive(false); });
                this.seq.SetEase(this.easeMode);
                this.seq.SetLoops(loopTimes);
            }
        }

        public override void TickPlay()
        {
            if (this.spr == null) return;

            switch (this.playMode)
            {
                case PlayMode.Normal:
                    this.DoTweenNormal();
                    break;
                case PlayMode.Reverse:
                    this.DoTweenReverse();
                    break;
                case PlayMode.PingPong:
                    this.DoTweenPingPong();
                    break;
                case PlayMode.Sequence:
                    this.DoTweenSequence();
                    break;
            }
        }
    }
    #endregion
    [SerializeField, ConditionalField(nameof(tSprColorOn))]
    private TweenSprColor tSprColor = new TweenSprColor();

    private void Awake()
    {
        // --- 初始Tween Postion
        this.tPosition.transform = this.transform;
        if (this.setOriginIsBeginValue) this.tPosition.Init();
        else this.tPosition.Init(this.transform.localPosition);

        // --- 初始Tween Rotation
        this.tRotation.transform = this.transform;
        if (this.setOriginIsBeginValue) this.tRotation.Init();
        else this.tRotation.Init(this.transform.localRotation.eulerAngles);

        // --- 初始Tween Scale
        this.tScale.transform = this.transform;
        if (this.setOriginIsBeginValue) this.tScale.Init();
        else this.tScale.Init(this.transform.localScale);

        // --- 初始Tween Size (RectTransform)
        this._rectTransform = this.transform.GetComponent<RectTransform>();
        if (this._rectTransform != null)
        {
            this.tSize.rectTransform = this._rectTransform;
            if (this.setOriginIsBeginValue) this.tSize.Init();
            else this.tSize.Init(this._rectTransform.sizeDelta);
        }

        // --- 初始Tween Alpha (CanvasGroup)
        this._cg = this.transform.GetComponent<CanvasGroup>();
        if (this._cg != null)
        {
            this.tAlpha.cg = this._cg;
            if (this.setOriginIsBeginValue) this.tAlpha.Init();
            else this.tAlpha.Init(this._cg.alpha);
        }

        // --- 初始Tween Image Color (Image)
        this._img = this.transform.GetComponent<Image>();
        if (this._img != null)
        {
            this.tImgColor.img = this._img;
            if (this.setOriginIsBeginValue) this.tImgColor.Init();
            else this.tImgColor.Init(this._img.color);
        }

        // --- 初始Tween Sprite Color (Sprite)
        this._spr = this.transform.GetComponent<SpriteRenderer>();
        if (this._spr != null)
        {
            this.tSprColor.spr = this._spr;
            if (this.setOriginIsBeginValue) this.tSprColor.Init();
            else this.tSprColor.Init(this._spr.color);
        }

        // 如果有勾選firstInit的話, 會在參數都初始後, 執行一次Reset定位各個參數
        if (this.firstInit)
        {
            this._ResetTween();
            Debug.Log("TweenAnim - FirstInit");
        }
    }

    private void Start()
    {
        // 要放在最後執行, 在參數初始後才能設置Active = false
        if (this.autoActive) this.gameObject.SetActive(false);
    }

    private void _ResetTween()
    {
        if (this.tPositionOn) this.tPosition.Reset();
        if (this.tRotationOn) this.tRotation.Reset();
        if (this.tScaleOn) this.tScale.Reset();
        if (this.tSizeOn) this.tSize.Reset();
        if (this.tAlphaOn) this.tAlpha.Reset();
        if (this.tImgColorOn) this.tImgColor.Reset();
        if (this.tSprColorOn) this.tSprColor.Reset();
    }

    private void _SetMainTweenByDuration(bool autoActive = false, TweenCallback callback = null)
    {
        List<TweenBase> tweens = new List<TweenBase>();
        if (this.tPositionOn) tweens.Add(this.tPosition);
        if (this.tRotationOn) tweens.Add(this.tRotation);
        if (this.tScaleOn) tweens.Add(this.tScale);
        if (this.tSizeOn) tweens.Add(this.tSize);
        if (this.tAlphaOn) tweens.Add(this.tAlpha);
        if (this.tImgColorOn) tweens.Add(this.tImgColor);
        if (this.tSprColorOn) tweens.Add(this.tSprColor);

        if (tweens.Count == 0) return;

        // 查找Duration最大值, 作為主要控制者
        TweenBase maxDurationTween = tweens.MaxBy((tween) => tween.duration);
        if (autoActive) maxDurationTween.autoActive = true;
        if (callback != null) maxDurationTween.callback = callback;

        //Debug.Log(string.Format("篩選出主要最大的Duration Tween => <color=#00FFFF>類型: {0}</color>, <color=#00FFFF>持續時間: {1}</color>", maxDurationTween.ToString().Replace("TweenAnimation+", ""), maxDurationTween.duration));
    }

    /// <summary>
    /// 透過呼叫的方式可以PlayTween (參數 = Optional)
    /// </summary>
    /// <param name="trigger">只有勾選PlayByEvent才會生效, 將會透過true or false進行自動播放與開關</param>
    /// <param name="callback">完成播放後的callback</param>
    public void PlayTween(bool trigger = false, TweenCallback callback = null)
    {
        this.gameObject.SetActive(true);
        if (!this.resetOnEnable) this._ResetTween(); // 如果有啟用resetOnEnable這邊就可以跳過, 將會交由OnEnable去進行Reset的動作

        this._SetMainTweenByDuration(this.autoActive, callback);

        #region Tween Postion On
        if (this.tPositionOn)
        {
            switch (this.tPosition.playMode)
            {
                case PlayMode.Normal:
                    if (this.playByEvent)
                    {
                        this.tPosition.DoTweenNormal(0, !trigger, trigger);
                    }
                    else if (this.tPosition.isInterval)
                    {
                        this.tPosition.DoTweenNormal();

                        this.tPosition.intervalTimer.Play();
                        this.tPosition.intervalTimer.SetTick(this.tPosition.intervalTime);
                    }
                    else
                    {
                        this.tPosition.DoTweenNormal(this.tPosition.loopTimes);
                    }
                    break;
                case PlayMode.Reverse:
                    if (this.playByEvent)
                    {
                        this.tPosition.DoTweenReverse(0, !trigger, trigger);
                    }
                    else if (this.tPosition.isInterval)
                    {
                        this.tPosition.DoTweenReverse();

                        this.tPosition.intervalTimer.Play();
                        this.tPosition.intervalTimer.SetTick(this.tPosition.intervalTime);
                    }
                    else
                    {
                        this.tPosition.DoTweenReverse(this.tPosition.loopTimes);
                    }
                    break;
                case PlayMode.PingPong:
                    if (this.playByEvent)
                    {
                        this.tPosition.DoTweenPingPong(0, !trigger, trigger);
                    }
                    else if (this.tPosition.isInterval)
                    {
                        this.tPosition.DoTweenPingPong();

                        this.tPosition.intervalTimer.Play();
                        this.tPosition.intervalTimer.SetTick(this.tPosition.intervalTime);
                    }
                    else
                    {
                        this.tPosition.DoTweenPingPong(this.tPosition.loopTimes);
                    }
                    break;
                case PlayMode.Sequence:
                    if (this.playByEvent)
                    {
                        this.tPosition.DoTweenSequence(0, !trigger, trigger);
                    }
                    else if (this.tPosition.isInterval)
                    {
                        this.tPosition.DoTweenSequence();

                        this.tPosition.intervalTimer.Play();
                        this.tPosition.intervalTimer.SetTick(this.tPosition.intervalTime);
                    }
                    else
                    {
                        this.tPosition.DoTweenSequence(this.tPosition.loopTimes);
                    }
                    break;
            }
        }
        #endregion

        #region Tween Rotation On
        if (this.tRotationOn)
        {
            switch (this.tRotation.playMode)
            {
                case PlayMode.Normal:
                    if (this.playByEvent)
                    {
                        this.tRotation.DoTweenNormal(0, !trigger, trigger);
                    }
                    else if (this.tRotation.isInterval)
                    {
                        this.tRotation.DoTweenNormal();

                        this.tRotation.intervalTimer.Play();
                        this.tRotation.intervalTimer.SetTick(this.tRotation.intervalTime);
                    }
                    else
                    {
                        this.tRotation.DoTweenNormal(this.tRotation.loopTimes);
                    }
                    break;
                case PlayMode.Reverse:
                    if (this.playByEvent)
                    {
                        this.tRotation.DoTweenReverse(0, !trigger, trigger);
                    }
                    else if (this.tRotation.isInterval)
                    {
                        this.tRotation.DoTweenReverse();

                        this.tRotation.intervalTimer.Play();
                        this.tRotation.intervalTimer.SetTick(this.tRotation.intervalTime);
                    }
                    else
                    {
                        this.tRotation.DoTweenReverse(this.tRotation.loopTimes);
                    }
                    break;
                case PlayMode.PingPong:
                    if (this.playByEvent)
                    {
                        this.tRotation.DoTweenPingPong(0, !trigger, trigger);
                    }
                    else if (this.tRotation.isInterval)
                    {
                        this.tRotation.DoTweenPingPong();

                        this.tRotation.intervalTimer.Play();
                        this.tRotation.intervalTimer.SetTick(this.tRotation.intervalTime);
                    }
                    else
                    {
                        this.tRotation.DoTweenPingPong(this.tRotation.loopTimes);
                    }
                    break;
                case PlayMode.Sequence:
                    if (this.playByEvent)
                    {
                        this.tRotation.DoTweenSequence(0, !trigger, trigger);
                    }
                    else if (this.tRotation.isInterval)
                    {
                        this.tRotation.DoTweenSequence();

                        this.tRotation.intervalTimer.Play();
                        this.tRotation.intervalTimer.SetTick(this.tRotation.intervalTime);
                    }
                    else
                    {
                        this.tRotation.DoTweenSequence(this.tRotation.loopTimes);
                    }
                    break;
            }
        }
        #endregion

        #region Tween Scale On
        if (this.tScaleOn)
        {
            switch (this.tScale.playMode)
            {
                case PlayMode.Normal:
                    if (this.playByEvent)
                    {
                        this.tScale.DoTweenNormal(0, !trigger, trigger);
                    }
                    else if (this.tScale.isInterval)
                    {
                        this.tScale.DoTweenNormal();

                        this.tScale.intervalTimer.Play();
                        this.tScale.intervalTimer.SetTick(this.tScale.intervalTime);
                    }
                    else
                    {
                        this.tScale.DoTweenNormal(this.tScale.loopTimes);
                    }
                    break;
                case PlayMode.Reverse:
                    if (this.playByEvent)
                    {
                        this.tScale.DoTweenReverse(0, !trigger, trigger);
                    }
                    else if (this.tScale.isInterval)
                    {
                        this.tScale.DoTweenReverse();

                        this.tScale.intervalTimer.Play();
                        this.tScale.intervalTimer.SetTick(this.tScale.intervalTime);
                    }
                    else
                    {
                        this.tScale.DoTweenReverse(this.tScale.loopTimes);
                    }
                    break;
                case PlayMode.PingPong:
                    if (this.playByEvent)
                    {
                        this.tScale.DoTweenPingPong(0, !trigger, trigger);
                    }
                    else if (this.tScale.isInterval)
                    {
                        this.tScale.DoTweenPingPong();

                        this.tScale.intervalTimer.Play();
                        this.tScale.intervalTimer.SetTick(this.tScale.intervalTime);
                    }
                    else
                    {
                        this.tScale.DoTweenPingPong(this.tScale.loopTimes);
                    }
                    break;
                case PlayMode.Sequence:
                    if (this.playByEvent)
                    {
                        this.tScale.DoTweenSequence(0, !trigger, trigger);
                    }
                    else if (this.tScale.isInterval)
                    {
                        this.tScale.DoTweenSequence();

                        this.tScale.intervalTimer.Play();
                        this.tScale.intervalTimer.SetTick(this.tScale.intervalTime);
                    }
                    else
                    {
                        this.tScale.DoTweenSequence(this.tScale.loopTimes);
                    }
                    break;
            }
        }
        #endregion

        #region Tween Size On (RectTransform)
        if (this.tSizeOn)
        {
            switch (this.tSize.playMode)
            {
                case PlayMode.Normal:
                    if (this.playByEvent)
                    {
                        this.tSize.DoTweenNormal(0, !trigger, trigger);
                    }
                    else if (this.tSize.isInterval)
                    {
                        this.tSize.DoTweenNormal();

                        this.tSize.intervalTimer.Play();
                        this.tSize.intervalTimer.SetTick(this.tSize.intervalTime);
                    }
                    else
                    {
                        this.tSize.DoTweenNormal(this.tSize.loopTimes);
                    }
                    break;
                case PlayMode.Reverse:
                    if (this.playByEvent)
                    {
                        this.tSize.DoTweenReverse(0, !trigger, trigger);
                    }
                    else if (this.tSize.isInterval)
                    {
                        this.tSize.DoTweenReverse();

                        this.tSize.intervalTimer.Play();
                        this.tSize.intervalTimer.SetTick(this.tSize.intervalTime);
                    }
                    else
                    {
                        this.tSize.DoTweenReverse(this.tSize.loopTimes);
                    }
                    break;
                case PlayMode.PingPong:
                    if (this.playByEvent)
                    {
                        this.tSize.DoTweenPingPong(0, !trigger, trigger);
                    }
                    else if (this.tSize.isInterval)
                    {
                        this.tSize.DoTweenPingPong();

                        this.tSize.intervalTimer.Play();
                        this.tSize.intervalTimer.SetTick(this.tSize.intervalTime);
                    }
                    else
                    {
                        this.tSize.DoTweenPingPong(this.tSize.loopTimes);
                    }
                    break;
                case PlayMode.Sequence:
                    if (this.playByEvent)
                    {
                        this.tSize.DoTweenSequence(0, !trigger, trigger);
                    }
                    else if (this.tSize.isInterval)
                    {
                        this.tSize.DoTweenSequence();

                        this.tSize.intervalTimer.Play();
                        this.tSize.intervalTimer.SetTick(this.tSize.intervalTime);
                    }
                    else
                    {
                        this.tSize.DoTweenSequence(this.tSize.loopTimes);
                    }
                    break;
            }
        }
        #endregion

        #region Tween Alpah On (CanvasGroup)
        if (this.tAlphaOn)
        {
            switch (this.tAlpha.playMode)
            {
                case PlayMode.Normal:
                    if (this.playByEvent)
                    {
                        this.tAlpha.DoTweenNormal(0, !trigger, trigger);
                    }
                    else if (this.tAlpha.isInterval)
                    {
                        this.tAlpha.DoTweenNormal();

                        this.tAlpha.intervalTimer.Play();
                        this.tAlpha.intervalTimer.SetTick(this.tAlpha.intervalTime);
                    }
                    else
                    {
                        this.tAlpha.DoTweenNormal(this.tAlpha.loopTimes);
                    }
                    break;
                case PlayMode.Reverse:
                    if (this.playByEvent)
                    {
                        this.tAlpha.DoTweenReverse(0, !trigger, trigger);
                    }
                    else if (this.tAlpha.isInterval)
                    {
                        this.tAlpha.DoTweenReverse();

                        this.tAlpha.intervalTimer.Play();
                        this.tAlpha.intervalTimer.SetTick(this.tAlpha.intervalTime);
                    }
                    else
                    {
                        this.tAlpha.DoTweenReverse(this.tAlpha.loopTimes);
                    }
                    break;
                case PlayMode.PingPong:
                    if (this.playByEvent)
                    {
                        this.tAlpha.DoTweenPingPong(0, !trigger, trigger);
                    }
                    else if (this.tAlpha.isInterval)
                    {
                        this.tAlpha.DoTweenPingPong();

                        this.tAlpha.intervalTimer.Play();
                        this.tAlpha.intervalTimer.SetTick(this.tAlpha.intervalTime);
                    }
                    else
                    {
                        this.tAlpha.DoTweenPingPong(this.tAlpha.loopTimes);
                    }
                    break;
                case PlayMode.Sequence:
                    if (this.playByEvent)
                    {
                        this.tAlpha.DoTweenSequence(0, !trigger, trigger);
                    }
                    else if (this.tAlpha.isInterval)
                    {
                        this.tAlpha.DoTweenSequence();

                        this.tAlpha.intervalTimer.Play();
                        this.tAlpha.intervalTimer.SetTick(this.tAlpha.intervalTime);
                    }
                    else
                    {
                        this.tAlpha.DoTweenSequence(this.tAlpha.loopTimes);
                    }
                    break;
            }
        }
        #endregion

        #region Tween Image Color On (Image)
        if (this.tImgColorOn)
        {
            switch (this.tImgColor.playMode)
            {
                case PlayMode.Normal:
                    if (this.playByEvent)
                    {
                        this.tImgColor.DoTweenNormal(0, !trigger, trigger);
                    }
                    else if (this.tImgColor.isInterval)
                    {
                        this.tImgColor.DoTweenNormal();

                        this.tImgColor.intervalTimer.Play();
                        this.tImgColor.intervalTimer.SetTick(this.tImgColor.intervalTime);
                    }
                    else
                    {
                        this.tImgColor.DoTweenNormal(this.tImgColor.loopTimes);
                    }
                    break;
                case PlayMode.Reverse:
                    if (this.playByEvent)
                    {
                        this.tImgColor.DoTweenReverse(0, !trigger, trigger);
                    }
                    else if (this.tImgColor.isInterval)
                    {
                        this.tImgColor.DoTweenReverse();

                        this.tImgColor.intervalTimer.Play();
                        this.tImgColor.intervalTimer.SetTick(this.tImgColor.intervalTime);
                    }
                    else
                    {
                        this.tImgColor.DoTweenReverse(this.tImgColor.loopTimes);
                    }
                    break;
                case PlayMode.PingPong:
                    if (this.playByEvent)
                    {
                        this.tImgColor.DoTweenPingPong(0, !trigger, trigger);
                    }
                    else if (this.tImgColor.isInterval)
                    {
                        this.tImgColor.DoTweenPingPong();

                        this.tImgColor.intervalTimer.Play();
                        this.tImgColor.intervalTimer.SetTick(this.tImgColor.intervalTime);
                    }
                    else
                    {
                        this.tImgColor.DoTweenPingPong(this.tImgColor.loopTimes);
                    }
                    break;
                case PlayMode.Sequence:
                    if (this.playByEvent)
                    {
                        this.tImgColor.DoTweenSequence(0, !trigger, trigger);
                    }
                    else if (this.tImgColor.isInterval)
                    {
                        this.tImgColor.DoTweenSequence();

                        this.tImgColor.intervalTimer.Play();
                        this.tImgColor.intervalTimer.SetTick(this.tImgColor.intervalTime);
                    }
                    else
                    {
                        this.tImgColor.DoTweenSequence(this.tImgColor.loopTimes);
                    }
                    break;
            }
        }
        #endregion

        #region Tween Sprite Color On (Sprite)
        if (this.tSprColorOn)
        {
            switch (this.tSprColor.playMode)
            {
                case PlayMode.Normal:
                    if (this.playByEvent)
                    {
                        this.tSprColor.DoTweenNormal(0, !trigger, trigger);
                    }
                    else if (this.tSprColor.isInterval)
                    {
                        this.tSprColor.DoTweenNormal();

                        this.tSprColor.intervalTimer.Play();
                        this.tSprColor.intervalTimer.SetTick(this.tSprColor.intervalTime);
                    }
                    else
                    {
                        this.tSprColor.DoTweenNormal(this.tSprColor.loopTimes);
                    }
                    break;
                case PlayMode.Reverse:
                    if (this.playByEvent)
                    {
                        this.tSprColor.DoTweenReverse(0, !trigger, trigger);
                    }
                    else if (this.tSprColor.isInterval)
                    {
                        this.tSprColor.DoTweenReverse();

                        this.tSprColor.intervalTimer.Play();
                        this.tSprColor.intervalTimer.SetTick(this.tSprColor.intervalTime);
                    }
                    else
                    {
                        this.tSprColor.DoTweenReverse(this.tSprColor.loopTimes);
                    }
                    break;
                case PlayMode.PingPong:
                    if (this.playByEvent)
                    {
                        this.tSprColor.DoTweenPingPong(0, !trigger, trigger);
                    }
                    else if (this.tSprColor.isInterval)
                    {
                        this.tSprColor.DoTweenPingPong();

                        this.tSprColor.intervalTimer.Play();
                        this.tSprColor.intervalTimer.SetTick(this.tSprColor.intervalTime);
                    }
                    else
                    {
                        this.tSprColor.DoTweenPingPong(this.tSprColor.loopTimes);
                    }
                    break;
                case PlayMode.Sequence:
                    if (this.playByEvent)
                    {
                        this.tSprColor.DoTweenSequence(0, !trigger, trigger);
                    }
                    else if (this.tSprColor.isInterval)
                    {
                        this.tSprColor.DoTweenSequence();

                        this.tSprColor.intervalTimer.Play();
                        this.tSprColor.intervalTimer.SetTick(this.tSprColor.intervalTime);
                    }
                    else
                    {
                        this.tSprColor.DoTweenSequence(this.tSprColor.loopTimes);
                    }
                    break;
            }
        }
        #endregion
    }

    private void Update()
    {
        if (this.playByEvent) return;

        float dt = Time.deltaTime;

        #region tPostion TickPlay
        if (this.tPositionOn && this.tPosition.isInterval)
        {
            this.tPosition.intervalTimer.UpdateTimer(dt);
            if (this.tPosition.intervalTimer.IsTickTimeout())
            {
                this.tPosition.TickPlay();
            }
        }
        #endregion

        #region tRoatation TickPlay
        if (this.tRotationOn && this.tRotation.isInterval)
        {
            this.tRotation.intervalTimer.UpdateTimer(dt);
            if (this.tRotation.intervalTimer.IsTickTimeout())
            {
                this.tRotation.TickPlay();
            }
        }
        #endregion

        #region tScale TickPlay
        if (this.tScaleOn && this.tScale.isInterval)
        {
            this.tScale.intervalTimer.UpdateTimer(dt);
            if (this.tScale.intervalTimer.IsTickTimeout())
            {
                this.tScale.TickPlay();
            }
        }
        #endregion

        #region tSize TickPlay (RectTransform)
        if (this.tSizeOn && this.tSize.isInterval)
        {
            this.tSize.intervalTimer.UpdateTimer(dt);
            if (this.tSize.intervalTimer.IsTickTimeout())
            {
                this.tSize.TickPlay();
            }
        }
        #endregion

        #region tAlpah TickPlay (CanvasGroup)
        if (this.tAlphaOn && this.tAlpha.isInterval)
        {
            this.tAlpha.intervalTimer.UpdateTimer(dt);
            if (this.tAlpha.intervalTimer.IsTickTimeout())
            {
                this.tAlpha.TickPlay();
            }
        }
        #endregion

        #region tImgColor TickPlay (Image)
        if (this.tImgColorOn && this.tImgColor.isInterval)
        {
            this.tImgColor.intervalTimer.UpdateTimer(dt);
            if (this.tImgColor.intervalTimer.IsTickTimeout())
            {
                this.tImgColor.TickPlay();
            }
        }
        #endregion

        #region tSprColor TickPlay (Sprite)
        if (this.tSprColorOn && this.tSprColor.isInterval)
        {
            this.tSprColor.intervalTimer.UpdateTimer(dt);
            if (this.tSprColor.intervalTimer.IsTickTimeout())
            {
                this.tSprColor.TickPlay();
            }
        }
        #endregion
    }

    private void OnEnable()
    {
        if (this.resetOnEnable) this._ResetTween();

        // 只要勾選playByEvent就直接return, 控制方式只靠Event去Invoke playTrigger(boolean)
        if (this.playByEvent) return;

        // 只要active=true就自動播放TweenAnim
        this.PlayTween();
    }

    private void OnDisable()
    {
        if (this.resetOnDisable) this._ResetTween();
    }

    private void OnDestroy()
    {
        // 銷毀物件時, 必須重置釋放Tween(Kill Tween)
        this._ResetTween();
    }
}
