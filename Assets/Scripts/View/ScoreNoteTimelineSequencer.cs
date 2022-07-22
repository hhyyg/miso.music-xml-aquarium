using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreNoteTimelineSequencer
{
    public ScoreNoteTimelineSequencer(
        IReadOnlyList<AquariumNoteView> noteViews
    )
    {
        this._noteViewEnumerator = noteViews.OrderBy(x => x.timeLine).GetEnumerator();
    }

    private IEnumerator<AquariumNoteView> _noteViewEnumerator;
    private bool _playing = false;

    public void Start()
    {
        this._playing = true;

        if (!this._noteViewEnumerator.MoveNext())
        {
            this._playing = false;
        }
    }

    public IEnumerable<AquariumNoteView> Advance(float excludeTimeLine)
    {
        if (!this._playing)
        {
            yield break;
        }

        while (this._noteViewEnumerator.Current.timeLine < excludeTimeLine
            && this._playing)
        {
            yield return this._noteViewEnumerator.Current;

            if (!this._noteViewEnumerator.MoveNext())
            {
                this._playing = false;
            }
        }
    }
}
