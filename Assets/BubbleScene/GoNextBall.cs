using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoNextBall : MonoBehaviour
{
    public ScoreRenderer ScoreRenderer;
    private float _ringTimeRest = 0;
    private float _ringDurationTime = 0;
    private float _ringStartTime = 0;

    private float _noteDuration = 0;
    private GameObject _toObject;
    private bool _moving = false;

    private Vector3 _originalPosition;
    private Vector3? _originalToObjectPosition;
    private AquariumNoteView _noteView;
    private Vector3 _originalScale;
    private float _fallY;


    public AnimationCurve Curve;
    private Transform _ballPrefab;


    void Start()
    {
        this._ballPrefab = this.transform.Find("BallPrefab");
    }
    void Update()
    {
        Move();
    }

    public void Schedule(
        AquariumNoteView noteView,
        float ringStartTime,
        GameObject toObject,
        float noteDuration)
    {
        this._noteView = noteView;
        this._ringStartTime = ringStartTime;
        this._ringDurationTime = noteDuration / ScoreRenderer.ScoreDivisions * (60 / ScoreRenderer.Bpm);
        this._ringTimeRest = this._ringDurationTime;
        this._fallY = BallController.Speed * this._ringDurationTime * -1;
        this._noteDuration = noteDuration;
        this._toObject = toObject;
        this._moving = false;
    }

    void Move()
    {
        if (this.ScoreRenderer?.AudioSource.isPlaying == true
            && this._ringStartTime <= this.ScoreRenderer?.AudioSource.time)
        {
            this.MoveToObject();
        }
    }

    void MoveToObject()
    {
        if (this._ringTimeRest > 0)
        {
            if (this._moving == false)
            {
                this._originalPosition = this.transform.position;
                this._originalToObjectPosition = this._toObject?.transform.position;
                this._moving = true;
                this._originalScale = this.transform.localScale;
            }
            this._ringTimeRest -= UnityEngine.Time.deltaTime;

            Vector3 toPosition;
            if (this._toObject == null)
            {
                toPosition = new Vector3(
                    this._originalPosition.x + (this._noteDuration * ScoreRenderer.DurationXUnit),
                    this._originalPosition.y + this._fallY,
                    this._originalPosition.z
                );
            }
            else
            {
                toPosition = new Vector3(
                    this._originalToObjectPosition.Value.x,
                    this._originalToObjectPosition.Value.y + this._fallY,
                    this._originalToObjectPosition.Value.z
                );
            }

            var rate = (this._ringDurationTime - this._ringTimeRest) / this._ringDurationTime;
            this.transform.position = Vector3.Lerp(
                this._originalPosition,
                toPosition,
                rate);

            var scaleRate = this.Curve.Evaluate(1 - rate);
            var scale = scaleRate * this._originalScale.x;
            this.transform.localScale = new Vector3(scale, scale, scale);

            var rotate = 100f * UnityEngine.Time.deltaTime;
            this._ballPrefab.transform.Rotate(new Vector3(rotate, -1 * rotate, rotate));
        }
        else
        {
            ResetTime();
        }
    }

    void ResetTime()
    {
        this._ringStartTime = 0;
        this._ringDurationTime = 0;
        this._ringTimeRest = 0;
        this._originalPosition = default(Vector3);
        this._originalToObjectPosition = null;
        this._moving = false;
        this._noteDuration = 0;
    }
}
