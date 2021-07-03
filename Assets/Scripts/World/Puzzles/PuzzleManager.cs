using System;
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

        private static Puzzle _currentPuzzle;
        private Camera _camera;

        private static PlayerController _player;

        private void Awake()
        {
            _camera = Camera.main;
            foreach (Puzzle puzzle in puzzles)
            {
                puzzle.OnPlayerEnter += PuzzleOnPlayerEnter;
                puzzle.SetEnabled(false);
            }

            _currentPuzzle = startPuzzle;
            
            _camera.transform.position = _currentPuzzle.PuzzleCameraPosition;
            player.transform.position = _currentPuzzle.PlayerSpawnPoint.position;
            _player = player;
        }

        private void Start()
        {
            _player.InputAction.PlayerMainMenu.Disable();
            _player.InputAction.Player.Enable();
        }

        private void OnDestroy()
        {
            foreach (Puzzle puzzle in puzzles)
            {
                puzzle.OnPlayerEnter -= PuzzleOnPlayerEnter;
            }
        }

        public static void ResetCurrentLevel()
        {
            _currentPuzzle.ResetObjects();
            PhysicsBody2D.SetGlobalGravityDirection(Vector2.up);
            WorldManager.GravityDirection = GravityDirection.Down;
            _player.transform.position = _currentPuzzle.PlayerSpawnPoint.position;
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
            WorldManager.GravityDirection = GravityDirection.Down;
        }
    }
}