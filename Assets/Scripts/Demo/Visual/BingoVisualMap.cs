using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BingoVisualMap : MonoBehaviour
{
    // Отвечает за визуализацию поля бинго

    private Transform[][] _points;
    private BingoMap _map;

    private Dictionary<(int, int), GameObject> _pieces;
    private GlobalFlags _globalFlags;

    // Визуальная часть карты
    [Header("Визуальная часть карты")]
    [SerializeField] private GameObject _visualTable;
    [SerializeField] private UniversalAnimator _visualTableShadow;
    [SerializeField] private float _border = 1.3f;

    // Анимация появления фишек
    [Header("Анимация появления фишек")]
    [SerializeField] private float _pieceAnimationSpeed = 1.0f;
    [SerializeField] private float _offsetYToSpawnPieces = -1f;

    // Анимация появления поля
    [Header("Анимация появления поля")]
    [SerializeField] private float _timeBeforeAnimationStart = 0.2f;
    [SerializeField] private float _tableAppearDuration = 1f;
    [SerializeField] private float _tableOvershootFactor = 1.2f;

    // Анимация конца раунда
    [Header("Анимация конца раунда")]
    [SerializeField] private float _timeBetweenHighliting = 0.4f;
    [SerializeField] private float _timeToHighlight = 0.35f;
    [SerializeField] private Color _colorHighlithed = Color.white;
    [SerializeField] private float _highlightedScaleFactor = 0.7f;

    [Header("Анимация выпадения фишек")]
    [SerializeField] private float _waitBeforeReset = 0.4f;
    [SerializeField] private float _offsetYToDisappearPieces = -10f;
    [SerializeField] private float _endRoundAnimationDuration = 1.3f;

    public float ToppestY => _points[_points.Length - 1][0].position.y;

    public static BingoVisualMap Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Init(Transform[][] p, BingoMap map, float distanceX, float distanceY)
    {
        _points = p;
        _map = map;
        _globalFlags = TBS_InitManager.Instance.GlobalFlags;

        _pieces = new();

        ConfigurateVisualTable(map.Width, map.Height, distanceX, distanceY);

        // Передаём через конфиг в фабрику данные про визуал фишек
        _map.OnElementAdded += HandleElementAdded;
        //_map.OnMapReset += HandleMapReset;
        _globalFlags.OnRoundEnded.AddListener(HandleRoundEnded);
    }

    private void OnDestroy()
    {
        if (_points != null)
        {
            _map.OnElementAdded -= HandleElementAdded;
            //_map.OnMapReset -= HandleMapReset;
            _globalFlags.OnRoundEnded.RemoveListener(HandleRoundEnded);
        }
    }

    private void ConfigurateVisualTable(int width, int height, float distanceX, float distanceY)
    {
        // Изменяет размер визуального поля - той синей штуки - под нужные параметры
        if (_visualTable == null)
        {
            Debug.LogError("BingoVisualMap: _visualTable не назначен!");
            return;
        }

        SpriteRenderer spriteRenderer = _visualTable.GetComponent<SpriteRenderer>();

        float scaleX = _visualTable.transform.lossyScale.x;
        float scaleY = _visualTable.transform.lossyScale.y;

        float worldWidth = (width - 1) * distanceX + _border * 2;
        float worldHeight = (height - 1) * distanceY + _border * 2;

        float localWidth = worldWidth / scaleX;
        float localHeight = worldHeight / scaleY;

        Vector2 targetSize = new Vector2(localWidth, localHeight);

        float centerX = (_points[0][0].position.x + _points[0][width - 1].position.x) / 2f;
        float centerY = (_points[0][0].position.y + _points[height - 1][0].position.y) / 2f;
        _visualTable.transform.position = new Vector3(centerX, centerY, _visualTable.transform.position.z);

        UniversalAnimator animator = _visualTable.GetComponent<UniversalAnimator>();

        animator.AnimateSpriteSizeWithOvershoot(Vector2.zero, targetSize, _tableAppearDuration, _tableOvershootFactor, _timeBeforeAnimationStart);
        _visualTableShadow.AnimateSpriteSizeWithOvershoot(Vector2.zero, targetSize, _tableAppearDuration, _tableOvershootFactor, _timeBeforeAnimationStart);
    }

    public float GetXByColumnId(int columnId)
    {
        if (columnId < 0 || columnId >= _points[0].Length) return 0;
        return _points[0][columnId].position.x;
    }

    public float GetYByRowId(int rowId)
    {
        if (rowId < 0 || rowId >= _points.Length) return 0;
        return _points[rowId][0].position.y;
    }

    private void HandleElementAdded(int x, int y, int playerId)
    {
        GameObject piecePrefab = VisualPieceFactory.GetPiece(playerId);
        if (piecePrefab == null)
        {
            Debug.LogError("Чё-то не то с фабрикой либо индексом");
            return;
        }

        GameObject piece = Instantiate(piecePrefab);
        Transform currentPoint = _points[y][x];
        piece.transform.position = new Vector2(currentPoint.position.x, _points[_points.Length - 1][0].position.y + _offsetYToSpawnPieces);

        piece.AddComponent<UniversalAnimator>().Animate(currentPoint.position, _pieceAnimationSpeed);
        _pieces[( y, x )] = piece;
        //Debug.Log($"Added piece at y={y} x={x}");
    }

    private void HandleRoundEnded(RuleWinResult result)
    {
        if (result.Result != GameWinCheckResult.Win)
        {
            HandleMapReset();
            return;
        }

        StartCoroutine(ResetMapAfterPiecesHighlighted((result as BingoWinResult).WinPieces));
    }

    private IEnumerator ResetMapAfterPiecesHighlighted(List<Piece> pieces)
    {
        WaitForSeconds w = new WaitForSeconds(_timeBetweenHighliting);

        foreach (Piece piece in pieces)
        {
            (int, int) key = (piece.Y, piece.X);

            if (!_pieces.ContainsKey(key))
            {
                Debug.Log($"Checking of y={piece.Y} x={piece.X} failed ((((");
                continue;
            }

            GameObject pieceGO = _pieces[key];

            UniversalAnimator animator = pieceGO.GetComponent<UniversalAnimator>();
            animator.PlayParticles();
            animator.InterpolateColor(_colorHighlithed, _timeToHighlight);
            animator.AnimateScale(pieceGO.transform.localScale * _highlightedScaleFactor, _timeToHighlight);

            yield return w;
        }

        HandleMapReset();
    }

    private void HandleMapReset()
    {
        int[] order = new int[_pieces.Count];
        for (int i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        var rng = new System.Random();
        for (int i = order.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        StartCoroutine(ClearMap(order, _endRoundAnimationDuration));
    }

    private IEnumerator ClearMap(int[] order, float totalTimeToClean)
    {
        yield return new WaitForSeconds(_waitBeforeReset);

        if (order.Length == 0)
        {
            _pieces.Clear();
            yield break;
        }

        List<GameObject> piecesList = _pieces.Values.ToList();

        if (order.Length == 1)
        {
            piecesList[order[0]].GetComponent<UniversalAnimator>()
                .AnimateWithOffset(new Vector2(0, _offsetYToDisappearPieces), _pieceAnimationSpeed, true);
            _pieces.Clear();
            yield break;
        }

        float elapsed = 0f;
        int currentIndex = 0;

        while (currentIndex < order.Length)
        {
            float progress = (float)currentIndex / (order.Length - 1);

            float easedProgress = (float)Math.Sqrt(progress);

            float targetTime = easedProgress * totalTimeToClean;

            while (elapsed < targetTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            piecesList[order[currentIndex]].GetComponent<UniversalAnimator>()
                .AnimateWithOffset(new Vector2(0, _offsetYToDisappearPieces), _pieceAnimationSpeed, true);

            currentIndex++;
        }

        _pieces.Clear();
    }
}