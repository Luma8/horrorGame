using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class ArmController : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AnimationClip clipIdle;
    [SerializeField] private AnimationClip clipWalk;
    [SerializeField] private AnimationClip clipRun;
    [SerializeField] private AnimationClip clipDraw;
    [SerializeField] private AnimationClip clipShoot;
    [SerializeField] private AnimationClip clipReload;

    private PlayerController _player;
    private PlayableGraph _graph;
    private AnimationMixerPlayable _mixer;
    private AnimationClipPlayable[] _clips;
    private int _current = -1;
    private bool _busy;

    private const int IDLE   = 0;
    private const int WALK   = 1;
    private const int RUN    = 2;
    private const int DRAW   = 3;
    private const int SHOOT  = 4;
    private const int RELOAD = 5;

    private void Awake()
    {
        _player = GetComponentInParent<PlayerController>();

        AnimationClip[] defs = { clipIdle, clipWalk, clipRun, clipDraw, clipShoot, clipReload };

        _graph = PlayableGraph.Create("Arms");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var output = AnimationPlayableOutput.Create(_graph, "Arms", GetComponent<Animator>());
        _mixer = AnimationMixerPlayable.Create(_graph, defs.Length);
        output.SetSourcePlayable(_mixer);

        _clips = new AnimationClipPlayable[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            if (defs[i] == null) continue;
            _clips[i] = AnimationClipPlayable.Create(_graph, defs[i]);
            _graph.Connect(_clips[i], 0, _mixer, i);
            _mixer.SetInputWeight(i, 0f);
        }

        _graph.Play();
    }

    private void OnDestroy() => _graph.Destroy();

    private void Start() => StartCoroutine(DrawOnStart());

    private void Update()
    {
        if (_player == null) return;

        LoopClip(IDLE);
        LoopClip(WALK);
        LoopClip(RUN);

        HandleActions();
        if (!_busy) HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 move   = _player.Input.Player.Move.ReadValue<Vector2>();
        bool moving    = move.sqrMagnitude > 0.01f;
        bool sprinting = _player.Input.Player.Sprint.IsPressed();

        if (moving && sprinting) SetClip(RUN);
        else if (moving)         SetClip(WALK);
        else                     SetClip(IDLE);
    }

    private void HandleActions()
    {
        if (!_busy && _player.Input.Player.Attack.WasPressedThisFrame())
            StartCoroutine(PlayAction(SHOOT, clipShoot));

        if (!_busy && _player.Input.Player.Interact.WasPressedThisFrame())
            StartCoroutine(PlayAction(RELOAD, clipReload));
    }

    private void SetClip(int idx)
    {
        if (_current == idx) return;
        _current = idx;

        for (int i = 0; i < _clips.Length; i++)
            _mixer.SetInputWeight(i, i == idx ? 1f : 0f);

        _clips[idx].SetTime(0);
        _clips[idx].Play();
    }

    private void LoopClip(int idx)
    {
        if (_current != idx) return;
        double duration = _clips[idx].GetDuration();
        if (duration > 0 && _clips[idx].GetTime() >= duration)
            _clips[idx].SetTime(0);
    }

    private IEnumerator DrawOnStart()
    {
        _busy = true;
        SetClip(DRAW);
        yield return new WaitForSeconds(clipDraw != null ? clipDraw.length : 1f);
        _busy = false;
    }

    private IEnumerator PlayAction(int idx, AnimationClip clip)
    {
        _busy = true;
        SetClip(idx);
        yield return new WaitForSeconds(clip != null ? clip.length - 0.05f : 0.5f);
        _busy = false;
    }
}
