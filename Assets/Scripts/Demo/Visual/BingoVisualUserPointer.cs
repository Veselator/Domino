using UnityEngine;

[DefaultExecutionOrder(100)]
public class BingoVisualUserPointer : MonoBehaviour
{
    // Визуализация указателя в бинго
    private PlayerControlls _playerControlls;
    private GlobalFlags _globalFlags;
    private BingoVisualMap _visualMap;
    private BingoMap _map;

    [SerializeField] private float _pieceSpeed = 2f;
    [SerializeField] private float _pieceYOffset = 1.2f;

    [SerializeField] private GameObject _pointer;
    [SerializeField] private GameObject[] _arrows;

    private UniversalAnimator _pointerAnimationComponent;
    private SpriteRenderer _cashedSpriteRenderer;

    public void Start()
    {
        _playerControlls = PlayerControlls.Instance;
        _globalFlags = TBS_InitManager.Instance.GlobalFlags;
        _visualMap = BingoVisualMap.Instance;
        _map = BingoMap.Instance as BingoMap;

        _playerControlls.OnMove += HandleMove;
        _globalFlags.OnHumansTurnStarted.AddListener(ShowPointer);
        _globalFlags.OnHumansTurnEnded.AddListener(HidePointer);

        Vector3 pointerPos = _pointer.transform.position;
        _pointer.transform.position = new Vector3(_visualMap.GetXByColumnId(_playerControlls.CurrentColumnId), _visualMap.ToppestY + _pieceYOffset, pointerPos.z);

        _pointerAnimationComponent = _pointer.GetComponent<UniversalAnimator>();
        _cashedSpriteRenderer = _pointer.GetComponent<SpriteRenderer>();

        _pointer.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_playerControlls != null)
        {
            _playerControlls.OnMove += HandleMove;
            _globalFlags.OnHumansTurnStarted.RemoveListener(ShowPointer);
            _globalFlags.OnHumansTurnEnded.RemoveListener(HidePointer);
        }
    }

    private void HandleMove(int columnId)
    {
        float yPos = _map.IsColumnFilled(columnId) ? _pointer.transform.position.y : _visualMap.GetYByRowId(_map.GetLengthOfColumn(columnId));
        Vector2 newPosition = new Vector2(_visualMap.GetXByColumnId(columnId), yPos);
        _pointerAnimationComponent.Animate(newPosition, _pieceSpeed);

        _arrows[0].SetActive(_playerControlls.IsPossibleToDoMove(1));
        _arrows[1].SetActive(_playerControlls.IsPossibleToDoMove(-1));
    }

    private void ForceToChangePosition(int columnId)
    {
        float yPos = _map.IsColumnFilled(columnId) ? _pointer.transform.position.y : _visualMap.GetYByRowId(_map.GetLengthOfColumn(columnId));
        Vector2 newPosition = new Vector2(_visualMap.GetXByColumnId(columnId), yPos);
        _pointerAnimationComponent.SetPosition(newPosition);
    }

    private void ShowPointer(int playerId)
    {
        // Меняем картинку на актуальную
        SpriteRenderer spriteRenderer = VisualPieceFactory.GetPiece(playerId).GetComponent<SpriteRenderer>();
        _cashedSpriteRenderer.sprite = spriteRenderer.sprite;
        _cashedSpriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, _cashedSpriteRenderer.color.a);

        _pointer.SetActive(true);
        ForceToChangePosition(_playerControlls.CurrentColumnId);
    }

    private void HidePointer(int playerId)
    {
        _pointer.SetActive(false);
    }
}
