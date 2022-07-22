using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static float Speed = 20f;
    public ScoreRenderer ScoreRenderer;
    public float MinY = -120;

    public float RotateSpeed = 5;

    public GameObject NextNote;

    private const float ShortRingTime = 0.5f;

    private float _ringStartTime = 0;
    public float RingStartTime { get { return _ringStartTime; } }
    private float _ringTimeRest = ShortRingTime;
    private AquariumNoteView _noteView;
    public AquariumNoteView NoteView { get { return _noteView; } }
    private ParticleSystem _particle;
    private Rigidbody _ballRigidBody;
    private MeshCollider _ballMeshCollider;

    void Start()
    {
        var ballPrefab = this.transform.Find("BallPrefab");
        this._ballRigidBody = ballPrefab.GetComponent<Rigidbody>();
        this._ballMeshCollider = ballPrefab.GetComponent<MeshCollider>();
        this._particle = ballPrefab.Find("Particle System").GetComponent<ParticleSystem>();
    }

    void Update()
    {
        this.Ring();

        if (transform.position.y < this.MinY)
        {
            this._ballMeshCollider.convex = true;
            this._ballRigidBody.isKinematic = false;
        }
        else
        {
            transform.Translate(
            0,
            -1 * BallController.Speed * UnityEngine.Time.deltaTime,
            0);
        }
    }

    void Ring()
    {
        if (this.ScoreRenderer?.AudioSource.isPlaying == true
            && this._ringStartTime <= this.ScoreRenderer?.AudioSource.time)
        {
            this.Animation();
        }
    }

    void Animation()
    {
        if (this._ringTimeRest > 0)
        {
            this._ringTimeRest -= UnityEngine.Time.deltaTime;

            if (!ScoreRenderer.IsLongNote(this._noteView.Note)
                && !this._particle.isPlaying)
            {
                this._particle.Play();
            }
        }
        else
        {
            if (this._particle.isPlaying)
            {
                this._particle.Stop();
            }
        }
    }

    public void SetNote(AquariumNoteView noteView, ScoreRendererBallPallet pallet)
    {
        this._noteView = noteView;
    }

    public void ScheduleRing(float ringStartTime)
    {
        this._ringStartTime = ringStartTime;
        this._ringTimeRest = ShortRingTime;
    }

    public void SetScale(Vector3 scale)
    {
        this.transform.localScale = new Vector3(1, 1, 1);
        var ballPrefab = this.transform.Find("BallPrefab");
        ballPrefab.localScale = scale;
        ballPrefab.rotation = Quaternion.Euler(0, 0, 0);
    }
}
