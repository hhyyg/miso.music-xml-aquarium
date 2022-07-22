// https://www.w3.org/2021/06/musicxml40/musicxml-reference/elements/note/
using System.Collections.Generic;

public struct ScoreNote : IMeasureChild
{
    public bool IsRest { get; set; }
    public Pitch? Pitch { get; set; }
    public int Duration { get; set; }
    public int Voice { get; set; }
    public string Type { get; set; }

    public List<Tie> TieList { get; set; }

    public bool IsChord { get; set; }
}
