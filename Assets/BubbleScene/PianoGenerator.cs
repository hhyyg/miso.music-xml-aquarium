using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PianoGenerator : MonoBehaviour
{
    public GameObject BlackKeyPrefab;
    public GameObject WhiteKeyPrefab;
    public GameObject PianoArea;

    private const float _keyMargin = 2f;
    private const int _keyCount = 128;
    private static readonly int[] _nextIsBlackKeyNumbers = { 0, 2, 5, 7, 9 };
    private static readonly int[] _blackKeyNumbers = { 1, 3, 6, 8, 10 };

    void Start()
    {
        RenderKeyBoard();
    }

    void RenderKeyBoard()
    {
        (float, float, float) nextPosition = (0, 0, 0);
        for (int iNoteNumber = 0; iNoteNumber < _keyCount; iNoteNumber++)
        {
            var pitchNumber = iNoteNumber % Pitch.NoteList.Count;
            var isBlackKey = _blackKeyNumbers.Contains(pitchNumber);

            GameObject noteObj = UnityEngine.Object.Instantiate<GameObject>(
                   isBlackKey ? this.BlackKeyPrefab : this.WhiteKeyPrefab,
                    new Vector3(0, 0, 0),
                    Quaternion.identity,
                    this.PianoArea.transform);
            noteObj.transform.localPosition = new Vector3(nextPosition.Item1, nextPosition.Item2, nextPosition.Item3);
            noteObj.name = iNoteNumber.ToString();

            if (_nextIsBlackKeyNumbers.Contains(pitchNumber))
            {
                nextPosition = (
                    nextPosition.Item1 + _keyMargin,
                    2.3f,
                    4.35f);
            }
            else if (_blackKeyNumbers.Contains(pitchNumber))
            {
                nextPosition = (
                    nextPosition.Item1 + _keyMargin,
                    0,
                    0);
            }
            else
            {
                nextPosition = (
                    nextPosition.Item1 + (_keyMargin * 2),
                    0,
                    0);
            }
        }

        // centering
        var parentPositionX = -1 * (nextPosition.Item1 / 2);
        this.PianoArea.transform.localPosition = new Vector3(
            parentPositionX,
            this.PianoArea.transform.localPosition.y,
            this.PianoArea.transform.localPosition.z
        );
    }
}
