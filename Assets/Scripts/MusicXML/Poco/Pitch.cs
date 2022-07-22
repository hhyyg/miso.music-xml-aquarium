using System.Collections.Generic;
using System.Linq;

// https://www.w3.org/2021/06/musicxml40/musicxml-reference/elements/pitch/
public struct Pitch
{
    // C ~ B
    public string Step { get; set; }
    // -1 ~ 9
    public int Octave { get; set; }

    // e.g., -1 for flat, 1 for sharp
    public int? Alter { get; set; }

    public override string ToString()
    {
        return $"{this.Step}{this.Octave} {this.Alter}";
    }

    public bool IsSameMidiNoteNumber(Pitch other)
    {
        return this.GetMidiNoteNumber() == other.GetMidiNoteNumber();
    }

    public int GetMidiNoteNumber()
    {

        var octaveValue = (this.Octave + 1) * 12; //  noteNumber 0は、C-1なので+1にする
        var stepValue = _noteList.IndexOf(this.Step);
        if (stepValue == -1)
        {
            throw new System.InvalidCastException($"invalid step: {this.Step}");
        }
        if (this.Alter != null)
        {
            stepValue += this.Alter.Value;
        }
        return octaveValue + stepValue;
    }

    public static Pitch GetPitchByMidiNoteNumber(int midiNoteNumber)
    {
        var octave = (midiNoteNumber / _noteList.Count) - 1; // noteNumber 0は、C-1なので、-1にする
        var pitchValue = midiNoteNumber % _noteList.Count; // 0~11を取得する
        var stepText = _noteList.ElementAt(pitchValue); // etc C, C# 
        return new Pitch()
        {
            Step = stepText.Substring(0, 1), // #があれば削除
            Octave = octave,
            Alter = stepText.Length > 1 ? 1 : null, // #があれば1
        };
    }

    public static IReadOnlyList<string> NoteList { get { return _noteList; } }
    private static List<string> _noteList = new List<string>()
    {
        "C",
        "C#",
        "D",
        "D#",
        "E",
        "F",
        "F#",
        "G",
        "G#",
        "A",
        "A#",
        "B",
    };
}
