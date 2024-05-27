using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] private PlayerDataScrObj playerData;


    [SerializeField]
    private float _activeTime;
    private float _timeActivated;
    private float _alpha;

    [SerializeField]
    private float _alphaInit = 1f;

    [SerializeField]
    private float _alphaMiltiplier;

    [SerializeField]
    private Color _color;


    private Transform _target;
    private Vector3 _offset;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _targetRenderer;

    private bool _isUpdate = false;


    public void SetTargetRenderer(SpriteRenderer renderer)
    {
        _targetRenderer = renderer;
    }
    public void SetTargetTransform(Transform target)
    {
        _target = target;
    }
    public void SetPositionOffset(Vector3 offset)
    {
        _offset = offset;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }



    public void Enable()
    {
        _isUpdate = true;
        this._spriteRenderer.enabled = true;

        _alpha = _alphaInit;
        _spriteRenderer.sprite = _targetRenderer.sprite;
        _spriteRenderer.material = _targetRenderer.material;
        transform.position = _target.position + _offset;
        transform.rotation = _target.rotation;
        _timeActivated = Time.time;

    }
    public void Disable()
    {
        _isUpdate = false;
        this._spriteRenderer.enabled = false;
    }


    private void Update()
    {
        print(playerData.faceDir);
        if (!_isUpdate)
            return;

        _alpha *= _alphaMiltiplier;
        _color.a = _alpha;
        _spriteRenderer.color = _color;
        if(playerData.faceDir == -1)
        {
            _spriteRenderer.flipX = false;
        }
        else
        {
            _spriteRenderer.flipX = true;

        }

        if (Time.time >= (_timeActivated + _activeTime))
        {
            AfterImagePool.Instance.AddIntoPool(this);
        }
    }

    public void DetachFromParent()
    {
        transform.parent = null;
    }
}

