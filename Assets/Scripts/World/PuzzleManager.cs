using Physics;
using UnityEngine;
using World.Puzzles;

namespace World
{
    public class PuzzleManager : MonoBehaviour
    {
        [SerializeField] private Puzzle startPuzzle;
        [SerializeField] private Puzzle[] puzzles;

        private Puzzle _currentPuzzle;

        private void Awake()
        {
            foreach (Puzzle puzzle in puzzles)
            {
                puzzle.OnPlayerEnter += PuzzleOnPlayerEnter;
                puzzle.PuzzleCamera.enabled = false;
                puzzle.SetEnabled(false);
            }

            _currentPuzzle = startPuzzle;
            _currentPuzzle.PuzzleCamera.enabled = true;
        }

        private void PuzzleOnPlayerEnter(Puzzle puzzle)
        {
            if (_currentPuzzle != null)
            {
                _currentPuzzle.PuzzleCamera.enabled = false;
                _currentPuzzle.SetEnabled(false);
            }

            _currentPuzzle = puzzle;
            _currentPuzzle.PuzzleCamera.enabled = true;
            _currentPuzzle.SetEnabled(true);

            PhysicsBody2D.SetGlobalGravityDirection(Vector2.up);
        }
    }
}