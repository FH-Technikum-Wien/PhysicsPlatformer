using Physics;
using Player;
using UnityEngine;

namespace World.Puzzles
{
    public class PuzzleManager : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Puzzle startPuzzle;
        [SerializeField] private Puzzle[] puzzles;

        private Puzzle _currentPuzzle;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            foreach (Puzzle puzzle in puzzles)
            {
                puzzle.OnPlayerEnter += PuzzleOnPlayerEnter;
                puzzle.SetEnabled(false);
            }

            _currentPuzzle = startPuzzle;
        }

        private void Start()
        {
            _camera.transform.position = _currentPuzzle.PuzzleCameraPosition;
            player.transform.position = _currentPuzzle.PlayerSpawnPoint.position;
        }

        private void PuzzleOnPlayerEnter(Puzzle puzzle)
        {
            if (_currentPuzzle != null)
            {
                _currentPuzzle.SetEnabled(false);
            }

            _currentPuzzle = puzzle;
            _camera.transform.position = _currentPuzzle.PuzzleCameraPosition;
            _currentPuzzle.SetEnabled(true);

            PhysicsBody2D.SetGlobalGravityDirection(Vector2.up);
        }
    }
}