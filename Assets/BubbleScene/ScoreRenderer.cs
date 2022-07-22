using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System.Linq;
using UnityEngine.InputSystem;

public class ScoreRenderer : MonoBehaviour
{
    public static float DurationXUnit = 0.5f;
    public static int ScoreDivisions = 120;
    public static int Bpm = 60;
    public TextAsset ScoreAsset;
    public GameObject BubblePrefab;
    public GameObject BallPrefab;
    public ScoreRendererBallPallet BallPallet;
    private static float OneNoteY = 1.6f;
    private static float YOfC4 = OneNoteY * Pitch.NoteList.Count * 5;
    private static float RenderBeatCount = 4;

    private ScoreNoteTimelineSequencer _scoreNoteSequencer;
    private float _nextRenderDuration = 0;
    private ObjectPool<GameObject> _poolBall;
    private ObjectPool<GameObject> _poolBubble;
    private AudioSource _audioSource;
    public AudioSource AudioSource { get { return _audioSource; } }
    private float _renderScoreBufferTime = 0.5f;

    private bool _isOrderAudioStart = false;

    private float _passedTimeSinceOrderAudioStart = 0;

    void Awake()
    {
        this._poolBall = new ObjectPool<GameObject>(
            () =>
            {
                var obj = UnityEngine.Object.Instantiate<GameObject>(this.BallPrefab);
                var controller = obj.GetComponent<BallController>();
                controller.ScoreRenderer = this;
                var goNextBall = obj.GetComponent<GoNextBall>();
                goNextBall.ScoreRenderer = this;
                obj.SetActive(false);
                return obj;
            },
            (GameObject obj) =>
            {
                obj.SetActive(true);
            },
            (GameObject obj) =>
            {
                obj.SetActive(false);
            },
            (GameObject obj) =>
            {
                Destroy(obj);
            },
            true,
            ScoreDivisions * 4 * 2);

        this._poolBubble = new ObjectPool<GameObject>(
            () =>
            {
                var obj = UnityEngine.Object.Instantiate<GameObject>(this.BubblePrefab);
                var controller = obj.GetComponent<BubbleController>();
                controller.ScoreRenderer = this;
                obj.SetActive(false);
                return obj;
            },
            (GameObject obj) =>
            {
                obj.SetActive(true);
            },
            (GameObject obj) =>
            {
                obj.SetActive(false);
            },
            (GameObject obj) =>
            {
                Destroy(obj);
            },
            true,
            ScoreDivisions * 4 * 2);
    }

    void Start()
    {
        this.BallPallet = GetComponent<ScoreRendererBallPallet>();
        this._audioSource = GetComponent<AudioSource>();
        var noteViews = GetNoteViews(this.ScoreAsset.text);
        this._scoreNoteSequencer = new ScoreNoteTimelineSequencer(noteViews);
        this._scoreNoteSequencer.Start();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            this._isOrderAudioStart = true;
        }

        if (this._isOrderAudioStart
        && !this._audioSource.isPlaying)
        {
            this._passedTimeSinceOrderAudioStart += UnityEngine.Time.deltaTime;
        }

        if (this._passedTimeSinceOrderAudioStart >= 1f)
        {
            RenderPartialScore();
        }

        if (this._passedTimeSinceOrderAudioStart >= 3f
            && !this._audioSource.isPlaying)
        {
            _audioSource.Play();
        }
    }

    public void ReleaseBall(GameObject obj)
    {
        this._poolBall.Release(obj);
    }

    public void ReleaseBubble(GameObject obj)
    {
        this._poolBubble.Release(obj);
    }

    void RenderPartialScore()
    {
        var nowTimeDuration = (this._audioSource.time + this._renderScoreBufferTime) * ScoreDivisions * Bpm / 60;

        if (this._nextRenderDuration <= nowTimeDuration)
        {
            var aheadDuration = this._nextRenderDuration + (ScoreDivisions * RenderBeatCount);

            var notes = this._scoreNoteSequencer.Advance(aheadDuration).ToList();
            if (notes == null)
            {
                return;
            }

            var ballNoteObjectList = new List<GameObject>(notes.Count());

            foreach (var noteView in notes)
            {
                if (noteView.Note.Pitch != null)
                {
                    var positionX = (noteView.timeLine - this._nextRenderDuration) * DurationXUnit;
                    var positionY = GetYFromPitch(noteView.Note.Pitch.Value);

                    if (noteView.Note.Voice == 1 || noteView.Note.Voice == 2)
                    {
                        positionX += -120f;
                        if (noteView.Note.Pitch.Value.Step == "A" && noteView.Note.Pitch.Value.Octave == 7)
                        {
                            positionY += -20f;
                        }
                        positionY += 30f;
                        var noteObj = this._poolBall.Get();
                        noteObj.transform.localPosition = new Vector3(positionX, positionY, 0);
                        noteObj.name = noteView.Id.ToString();

                        var ballController = noteObj.GetComponent<BallController>();
                        ballController.SetNote(noteView, this.BallPallet);

                        if (ScoreRenderer.IsLongNote(noteView.Note))
                        {
                            ballController.SetScale(new Vector3(8f, 8f, 8f));
                        }
                        else
                        {
                            ballController.SetScale(new Vector3(4f, 4f, 4f));
                        }

                        var ringStartTime = noteView.timeLine * (60f / Bpm / ScoreDivisions);
                        ballController.ScheduleRing(ringStartTime);

                        ballNoteObjectList.Add(noteObj);
                    }
                    else
                    {
                        positionX += -120f;
                        positionY += -60f;
                        var noteObj = this._poolBubble.Get();
                        noteObj.transform.localPosition = new Vector3(positionX, positionY, 0);
                        noteObj.name = noteView.Note.Pitch?.ToString();
                    }
                }

            }

            // relation
            foreach (var noteView in notes)
            {
                if (ScoreRenderer.IsLongNote(noteView.Note) == false)
                {
                    continue;
                }

                var targetObj = ballNoteObjectList.FirstOrDefault(x => x.name == noteView.Id.ToString());
                if (targetObj == null)
                {
                    // left LongTone
                    continue;
                }
                var targetBallController = targetObj.GetComponent<BallController>();
                var goNextBall = targetObj.GetComponent<GoNextBall>();

                GameObject foundNextObj = null;
                BallController foundNextBallController = null;
                int findId = noteView.NextNoteId;
                while (foundNextObj == null)
                {
                    var currentObj = ballNoteObjectList.FirstOrDefault(x => x.name == findId.ToString());
                    if (currentObj == null)
                    {
                        break;
                    }
                    var currentBallController = currentObj.GetComponent<BallController>();
                    if (currentBallController.NoteView.IsChord == false)
                    {
                        foundNextObj = currentObj;
                        foundNextBallController = currentBallController;
                    }
                    findId = currentBallController.NoteView.NextNoteId;
                }

                if (foundNextObj != null && targetBallController.NoteView.Voice == foundNextBallController.NoteView.Voice)
                {
                    goNextBall.Schedule(
                        targetBallController.NoteView,
                        targetBallController.RingStartTime,
                        foundNextObj,
                        targetBallController.NoteView.Note.Duration);
                }
                else
                {
                    goNextBall.Schedule(
                        targetBallController.NoteView,
                        targetBallController.RingStartTime,
                        null,
                        targetBallController.NoteView.Note.Duration);
                }
            }

            this._nextRenderDuration = aheadDuration;
        }
    }

    float GetYFromPitch(Pitch pitch)
    {
        return pitch.GetMidiNoteNumber() * OneNoteY - YOfC4;
    }

    IReadOnlyList<AquariumNoteView> GetNoteViews(string musicXmlText)
    {
        var score = MusicXMLParser.GetScorePartwise(musicXmlText);

        int timeLineCursor = 0;
        var notesSortedByScore = new List<AquariumNoteView>();
        var id = 0;

        foreach (var part in score.ScoreParts)
        {
            foreach (var measure in part.MeasureList)
            {
                foreach (IMeasureChild child in measure.Children)
                {
                    if (child is ScoreNote note)
                    {
                        float y = 0;
                        // y
                        if (note.Pitch != null)
                        {
                            y = note.Pitch.Value.GetMidiNoteNumber() * OneNoteY - YOfC4;
                        }

                        // x
                        int willConsumeDuration = note.Duration;

                        if (note.IsChord)
                        {
                            // 開始位置は、前の音と同じ位置
                            timeLineCursor = notesSortedByScore[Math.Max(notesSortedByScore.Count - 1, 0)].timeLine;
                        }

                        // instantiate
                        if (note.Pitch != null)
                        {
                            var noteView = new AquariumNoteView()
                            {
                                Id = id,
                                timeLine = timeLineCursor,
                                Note = note,
                                Voice = note.Voice,
                                IsChord = note.IsChord
                            };
                            id += 1;
                            noteView.NextNoteId = id;
                            notesSortedByScore.Add(noteView);
                        }
                        if (note.Pitch != null)
                        {
                            // TODO
                        }

                        timeLineCursor += willConsumeDuration;
                    }
                    else if (child is Backup backup)
                    {
                        timeLineCursor -= backup.Duration;
                    }
                    else if (child is Forward forward)
                    {
                        timeLineCursor += forward.Duration;
                    }
                }
            }
        }

        return notesSortedByScore;
    }

    public static bool IsLongNote(ScoreNote note)
    {
        return note.Type == "eighth"
            || note.Type == "quarter"
            || note.Type == "half"
            || note.Type == "whole";
    }
}



