using System.Collections.Generic;
public struct Score
{
    public int? Tempo { get; set; }

    public List<ScorePart> ScoreParts { get; set; }
}