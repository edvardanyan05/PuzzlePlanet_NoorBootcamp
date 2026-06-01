using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("Board Position")]
    public Transform currentPos;
    public Transform correctPos;

    [Header("Visual")]
    public Transform visual;
    public Transform plane;

    [Header("Grid Position")]
    public int correctRow;
    public int correctCol;

    [Header("Settings")]
    public float liftAmount = 0.5f;
    public float swapDistance = 2f;

    [Header("Effects")]
    public GameObject sparklePrefab;

    private Vector3 offset;
    private bool isDragging = false;
    private bool isLocked = false;
    private BoardManager boardManager;
    private bool isFrozen = false;


    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        CheckIfCorrect();
    }

    void OnMouseDown()
    {
        if (isLocked) return;
        if (isFrozen) return;
        if (boardManager.IsPaused()) return;
        boardManager.ResetHintTimer();
        isDragging = true;
        visual.localPosition = Vector3.up * liftAmount;
        offset = transform.position - GetMouseWorldPosition();
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 newPos = GetMouseWorldPosition() + offset;
        newPos.y = currentPos.position.y;
        transform.position = newPos;
    }

    void OnMouseUp()
    {
        if (isLocked) return;
        if (isFrozen) return;
        if (boardManager.IsPaused()) return;
        isDragging = false;
        visual.localPosition = Vector3.zero;
        Piece nearestPiece = GetNearestPiece();
        if (nearestPiece != null && !nearestPiece.isLocked && !nearestPiece.IsFrozen())
            Swap(nearestPiece);
        else
            ReturnToPosition();
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    Piece GetNearestPiece()
    {
        Piece[] allPieces = FindObjectsOfType<Piece>();
        Piece nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Piece piece in allPieces)
        {
            if (piece == this) continue;
            float distance = Vector3.Distance(transform.position, piece.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = piece;
            }
        }

        return shortestDistance <= swapDistance ? nearest : null;
    }

    void Swap(Piece other)
    {
        Transform tempPos = currentPos;
        currentPos = other.currentPos;
        other.currentPos = tempPos;

        transform.position = currentPos.position;
        other.transform.position = other.currentPos.position;

        CheckIfCorrect();
        other.CheckIfCorrect();
        AudioManager.Instance?.PlaySwap();
    }

    void ReturnToPosition()
    {
        transform.position = currentPos.position;
    }

    void CheckIfCorrect()
    {
        if (currentPos == correctPos)
        {
            isLocked = true;
            PlaySparkle();
            Debug.Log(gameObject.name + " LOCKED");
            AudioManager.Instance?.PlayPieceLocked();
        }
        else
        {
            isLocked = false;
        }
        boardManager.CheckWin();
    }

    void PlaySparkle()
    {
        if (sparklePrefab == null) return;

        Vector3 spawnPos = visual.position + Vector3.up * 0.3f;
        GameObject sparkle = Instantiate(sparklePrefab, spawnPos, Quaternion.identity);
        sparkle.transform.localScale = visual.lossyScale * 3f;

        ParticleSystem ps = sparkle.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        Destroy(sparkle, 2f);
    }

    public bool IsLocked() => isLocked;
    public void DisableInteraction() => isLocked = true;
    public void Freeze() => isFrozen = true;
    public void Unfreeze() => isFrozen = false;
    public bool IsFrozen() => isFrozen;
}