using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

public static class MusicXMLParser
{
    /// <summary>
    /// MusicXMLのテキストから、音符オブジェクトに変換します。
    /// </summary>
    /// <returns>The score partwise.</returns>
    /// <param name="text">Text.</param>
    public static Score GetScorePartwise(string text)
    {
        var doc = new XmlDocument();
        doc.LoadXml(text);

        var score = new Score();
        var partIdList = GetScorePartIdList(doc);
        score.ScoreParts = partIdList.Select(partId =>
        {
            return new ScorePart()
            {
                MeasureList = GetMeasures(partId, doc).ToArray(),
            };
        }).ToList();
        score.Tempo = GetScoreTempo(doc);

        return score;
    }

    private static int GetScoreTempo(XmlDocument doc)
    {
        //get first tempo
        //not support multiple tempo
        var soundElements = doc.SelectNodes("score-partwise/part/measure/direction/sound");
        if (soundElements == null)
        {
            throw new Exception("not found sound");
        }
        var tempo = ((IEnumerable)soundElements).Cast<XmlNode>().First().Attributes["tempo"].InnerText;
        return Int32.Parse(tempo);
    }

    private static IEnumerable<Measure> GetMeasures(string partId, XmlDocument doc)
    {
        var measureNodes = doc.SelectNodes($"score-partwise/part[@id='{partId}']/measure");

        foreach (XmlNode measureNode in measureNodes)
        {
            var measure = new Measure();
            measure.Number = Int64.Parse(measureNode.Attributes["number"].InnerText);
            measure.Attribute = GetAttribute(measureNode.SelectSingleNode("attributes"));
            measure.Children = GetMeasureChildren(measureNode).ToArray();
            yield return measure;
        }
    }

    private static IEnumerable<IMeasureChild> GetMeasureChildren(XmlNode measureNode)
    {
        foreach (XmlNode measureChildNode in measureNode)
        {
            if (measureChildNode.Name == "note")
            {
                yield return GetScoreNote(measureChildNode);
            }
            else if (measureChildNode.Name == "backup")
            {
                yield return GetBackup(measureChildNode);
            }
            else if (measureChildNode.Name == "forward")
            {
                yield return GetForward(measureChildNode);
            }
        }
    }

    private static IEnumerable<ScoreNote> GetNotes(XmlNode measureNode)
    {
        var noteNodes = measureNode.SelectNodes("note");

        foreach (XmlNode noteNode in noteNodes)
        {
            yield return GetScoreNote(noteNode);
        }
    }

    private static ScoreNote GetScoreNote(XmlNode noteNode)
    {
        var scoreNote = new ScoreNote()
        {
            Pitch = GetPitch(noteNode.SelectSingleNode("pitch")),
            Duration = Int32.Parse(noteNode.SelectSingleNode("duration").InnerText),
            Type = noteNode.SelectSingleNode("type")?.InnerText,
            IsRest = (noteNode.SelectSingleNode("rest") != null),
            IsChord = noteNode.SelectSingleNode("chord") != null,
            Voice = Int32.Parse(noteNode.SelectSingleNode("voice").InnerText),
        };

        var tieNodes = noteNode.SelectNodes("tie");
        if (tieNodes != null)
        {
            scoreNote.TieList = new List<Tie>();
            foreach (XmlNode tieNode in tieNodes)
            {
                var tie = GetTie(tieNode);
                if (tie != null)
                {
                    scoreNote.TieList.Add(tie.Value);
                }
            }
        }

        return scoreNote;
    }

    private static Tie? GetTie(XmlNode tieNode)
    {
        if (tieNode == null)
        {
            return null;
        }

        return new Tie()
        {
            Type = tieNode.Attributes["type"].InnerText,
        };
    }

    private static Backup GetBackup(XmlNode noteNode)
    {
        return new Backup()
        {
            Duration = Int32.Parse(noteNode.SelectSingleNode("duration").InnerText),
        };
    }

    private static Forward GetForward(XmlNode noteNode)
    {
        return new Forward()
        {
            Duration = Int32.Parse(noteNode.SelectSingleNode("duration").InnerText),
        };
    }

    private static MeasureAttribute? GetAttribute(XmlNode attributesNode)
    {
        if (attributesNode == null)
        {
            return null;
        }
        return new MeasureAttribute()
        {
            Divisions = attributesNode.SelectSingleNode("divisions") == null ? null : Int32.Parse(attributesNode.SelectSingleNode("divisions").InnerText),
            Time = attributesNode.SelectSingleNode("time") == null ? null : new ScoreTime()
            {
                Beats = Int32.Parse(attributesNode.SelectSingleNode("time/beats").InnerText),
                BeatType = Int32.Parse(attributesNode.SelectSingleNode("time/beat-type").InnerText),
            },
        };
    }

    private static Pitch? GetPitch(XmlNode pitchNode)
    {
        if (pitchNode == null)
        {
            return null;
        }
        return new Pitch()
        {
            Step = pitchNode.SelectSingleNode("step").InnerText,
            Octave = Int32.Parse(pitchNode.SelectSingleNode("octave").InnerText),
            Alter = pitchNode.SelectSingleNode("alter") == null ? null : Int32.Parse(pitchNode.SelectSingleNode("alter").InnerText),
        };
    }

    private static IEnumerable<string> GetScorePartIdList(XmlDocument doc)
    {
        var scorePartNodes = doc.SelectNodes("score-partwise/part-list/score-part");
        foreach (XmlNode scorePartNode in scorePartNodes)
        {
            var partId = scorePartNode.Attributes["id"].InnerText;
            yield return partId;
        }
    }
}