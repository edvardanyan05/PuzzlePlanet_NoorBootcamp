using UnityEngine;
using TMPro;
using DG.Tweening;

public class BoardManager : MonoBehaviour
{
    [Header("Pieces")]
    public Piece[] allPieces;

    [Header("Timer")]
    public float timeLimit = 30f;
    public TextMeshProUGUI timerText;

    [Header("Win Animation")]
    public int gridWidth = 2;
    public int gridHeight = 2;
    public float animationDuration = 0.6f;

    [Header("Win/Lose Panels")]
    public RectTransform WinPanel;
    public RectTransform LosePanel;

    [Header("Hint")]
    public float hintDelay = 2f;
    public float hintRotateAngle = 15f;
    public float hintRotateSpeed = 0.6f;

    [Header("Timer Bar")]
    public UnityEngine.UI.Image timerBar;

    [Header("Frozen Pieces")]
    public Piece[] frozenPieces;
    public int unlockAfterLocked = 0;
    public GameObject[] chains;

    private float currentTime;
    private bool hasWon = false;
    private bool hasLost = false;
    private bool isPaused = false;

    private float idleTimer = 0f;
    private bool isHintActive = false;
    private Piece hintPiece1 = null;
    private Piece hintPiece2 = null;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        currentTime = timeLimit;
        if (frozenPieces != null)
            foreach (Piece piece in frozenPieces)
                piece.Freeze();
    }

    void Update()
    {
        if (hasWon || hasLost || isPaused) return;

        currentTime -= Time.deltaTime;
        timerText.text = Mathf.Ceil(currentTime).ToString();
        if (timerBar != null)
            timerBar.fillAmount = currentTime / timeLimit;
        if (currentTime <= 0f)
        {
            Lose();
            return;
        }

        bool isTouching = Input.touchCount > 0 || Input.GetMouseButton(0);

        if (isTouching)
        {
            ResetHintTimer();
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= hintDelay && !isHintActive)
                ShowHint();
        }
    }

    public void PauseTimer()
    {
        isPaused = true;
        ResetHintTimer();
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    public void ResetHintTimer()
    {
        idleTimer = 0f;
        StopHint();
    }

    void ShowHint()
    {
        Piece wrongPiece = null;
        Piece targetPiece = null;

        foreach (Piece piece in allPieces)
        {
            if (piece.IsLocked() || piece.IsFrozen()) continue;

            Piece target = null;
            foreach (Piece other in allPieces)
            {
                if (other != piece && other.currentPos == piece.correctPos && !other.IsFrozen())
                {
                    target = other;
                    break;
                }
            }

            if (target != null)
            {
                wrongPiece = piece;
                targetPiece = target;
                break;
            }
        }

        if (wrongPiece == null || targetPiece == null) return;

        hintPiece1 = wrongPiece;
        hintPiece2 = targetPiece;
        isHintActive = true;

        StartHintAnimation(hintPiece1.visual);
        StartHintAnimation(hintPiece2.visual);
    }

    void StartHintAnimation(Transform visual)
    {
        visual.DOLocalRotate(new Vector3(0, -hintRotateAngle, 0), hintRotateSpeed)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                visual.DOLocalRotate(new Vector3(0, hintRotateAngle, 0), hintRotateSpeed)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            });
    }

    void StopHint()
    {
        if (!isHintActive) return;

        if (hintPiece1 != null)
        {
            hintPiece1.visual.DOKill();
            hintPiece1.visual.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.OutSine);
        }

        if (hintPiece2 != null)
        {
            hintPiece2.visual.DOKill();
            hintPiece2.visual.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.OutSine);
        }

        hintPiece1 = null;
        hintPiece2 = null;
        isHintActive = false;
    }

    public void CheckWin()
    {
        if (hasWon || hasLost) return;

        CheckFrozenUnlock();

        foreach (Piece piece in allPieces)
            if (!piece.IsLocked()) return;

        hasWon = true;
        StopHint();
        Win();
    }

    void Win()
    {
        AudioManager.Instance?.PlayWinMusic();
        foreach (Piece piece in allPieces)
            piece.DisableInteraction();

        Renderer planeRenderer = allPieces[0].plane.GetComponent<Renderer>();
        float autoSizeX = planeRenderer.bounds.size.x;
        float autoSizeZ = planeRenderer.bounds.size.z;

        Vector3 center = Vector3.zero;
        foreach (Piece piece in allPieces)
            center += piece.plane.position;
        center /= allPieces.Length;

        Sequence seq = DOTween.Sequence();

        foreach (Piece piece in allPieces)
        {
            float targetX = center.x + (piece.correctCol - (gridWidth - 1) / 2f) * autoSizeX;
            float targetZ = center.z + (piece.correctRow - (gridHeight - 1) / 2f) * autoSizeZ;
            Vector3 targetPos = new Vector3(targetX, center.y, targetZ);

            seq.Join(
                piece.plane
                    .DOMove(targetPos, animationDuration)
                    .SetEase(Ease.InOutQuad)
            );
        }

        seq.OnComplete(() =>
        {
            WinPanel.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutCubic).SetDelay(1.5f);
            Debug.Log("Ready to show Win UI!");
        });
    }

    void Lose()
    {
        AudioManager.Instance?.PlayLoseMusic();
        hasLost = true;
        StopHint();
        LosePanel.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutCubic);
        Debug.Log("YOU LOSE!");
    }

    private bool frozenUnlocked = false;
    void CheckFrozenUnlock()
    {
        if (frozenUnlocked) return;
        if (frozenPieces == null || frozenPieces.Length == 0) return;
        if (unlockAfterLocked <= 0) return;

        int lockedCount = 0;
        foreach (Piece piece in allPieces)
            if (piece.IsLocked()) lockedCount++;

        if (lockedCount >= unlockAfterLocked)
        {
            frozenUnlocked = true;
            AudioManager.Instance?.PlayChainBreak();

            foreach (Piece piece in frozenPieces)
                piece.Unfreeze();

            foreach (GameObject chain in chains)
            {
                if (chain == null) continue;
                chain.transform.DOScale(Vector3.zero, 0.4f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => chain.SetActive(false));
            }
        }
    }

    public bool IsPaused() => isPaused;

}