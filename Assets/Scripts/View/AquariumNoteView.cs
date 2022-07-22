public struct AquariumNoteView
{
    public int Id { get; set; }
    public int Voice { get; set; }
    // unit: duration
    public int timeLine { get; set; }
    public ScoreNote Note { get; set; }

    public int NextNoteId { get; set; }
    public bool IsChord { get; set; }
}