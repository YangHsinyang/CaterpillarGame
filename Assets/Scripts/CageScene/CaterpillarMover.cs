using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaterpillarMover : MonoBehaviour
{
    // ===== References =====
    [Header("References")]
    public CaterpillarController controller;
    public BranchStatus branchStatus;
    public CaterpillarStatus caterpillarStatus;
    public Transform actor;
    [SerializeField] private Animator anim;

    // ===== Parameters =====
    [Header("Move Parameters")]
    public float moveSpeed = 0.5f;
    public float arriveThreshold = 0.01f;

    [Header("Timing")]
    public float leafEatDelay = 1.0f;

    // ===== Runtime State =====
    private bool isRunning = false;
    private bool isTurningByAnim = false;

    // ===== Animator Hashes =====
    private readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private readonly int HashSleepTrigger = Animator.StringToHash("Sleep");
    
    private void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    // ===== Entry Point =====
    public void RunPlan(CaterpillarNavigator.PlanResult plan)
    {
        if (isRunning) return;
        if (plan == null || plan.routes == null || plan.routes.Count == 0) return;

        StartCoroutine(RunPlanCo(plan));
    }

    // ===== Main Coroutine =====
    private IEnumerator RunPlanCo(CaterpillarNavigator.PlanResult plan)
    {
        isRunning = true;

        var routes = plan.routes;
        var targets = plan.targets;

        // Move to each leaf
        for (int i = 0; i < targets.Count; i++)
        {
            var route = routes[i];
            var leafId = targets[i];
            yield return MoveAlongLeg(route, leafId);
        }

        // Move to sleep point
        var lastRoute = routes[routes.Count - 1];
        yield return MoveAlongLeg(lastRoute, null);

        isRunning = false;
    }

    // ===== Execute One Leg =====
    private IEnumerator MoveAlongLeg(List<RoutePoint> route, string leafId)
    {
        if (route == null || route.Count == 0) yield break;
        if (!actor) yield break;

        // U-turn at departure if needed
        RoutePoint nextTarget = route[1];   // route[0] = current position, route[1] = next target
        yield return DoUTurnIfNeeded(controller.currentPoint, nextTarget);

        // Move through each point (skip route[0] since it's the current position)
        for (int i = 1; i < route.Count; i++)
        {
            RoutePoint target = route[i];
            if (!target) continue;

            yield return MoveToPoint(target);

            RoutePoint nextPoint = (i + 1 < route.Count) ? route[i + 1] : null;
            yield return HandleArrival(target, nextPoint, leafId);

            controller.currentPoint = target;
        }
    }

    // ===== Move to Single Point =====
    private IEnumerator MoveToPoint(RoutePoint target)
    {
        if (!target) yield break;

        Vector3 goalPos = target.transform.position;
        anim.SetBool(HashIsMoving, true);

        while (Vector3.Distance(actor.position, goalPos) > arriveThreshold)
        {
            // Wait if turning by animation
            if (isTurningByAnim)
            {
                anim.SetBool(HashIsMoving, false);
                yield return null;
                continue;
            }

            // Move forward
            Vector3 dir = (goalPos - actor.position).normalized;
            actor.position += dir * moveSpeed * Time.deltaTime;
            yield return null;
        }

        // Snap to exact position
        actor.position = goalPos;
        anim.SetBool(HashIsMoving, false);
    }

    // ===== Handle Arrival at Point =====
    private IEnumerator HandleArrival(RoutePoint current, RoutePoint next, string leafId)
    {
        if (!current) yield break;

        switch (current.pointType)
        {
            case RoutePointType.Turn:
                yield return HandleTurnArrival(current, next);
                break;

            case RoutePointType.Leaf:
                yield return HandleLeafArrival(leafId);
                break;

            case RoutePointType.Sleep:
                yield return HandleSleepArrival();
                break;
        }
    }

    // ===== Turn Point Arrival =====
    private IEnumerator HandleTurnArrival(RoutePoint current, RoutePoint next)
    {
        Debug.Log($"HandleTurnArrival: next '{next}'");
        if (!next) yield break;

        string turnTrigger = DecideTurnTrigger(current, next);

        Debug.Log($"HandleTurnArrival: turnTrigger '{turnTrigger}'");

        if (!string.IsNullOrEmpty(turnTrigger))
        {
            yield return PlayTurnAndWait(turnTrigger);
        }
    }

    // ===== Leaf Point Arrival =====
    private IEnumerator HandleLeafArrival(string leafId)
    {
        if (!string.IsNullOrEmpty(leafId))
        {
            if (branchStatus != null) branchStatus.SetEaten(leafId);
            if (caterpillarStatus != null) caterpillarStatus.IncrementEatenToday();
        }

        yield return new WaitForSeconds(leafEatDelay);
    }

    // ===== Sleep Point Arrival =====
    private IEnumerator HandleSleepArrival()
    {
        if (anim)
        {
            anim.ResetTrigger(HashSleepTrigger);
            anim.SetTrigger(HashSleepTrigger);
        }
        yield break;
    }

    // ===== U-Turn at Departure =====
    private IEnumerator DoUTurnIfNeeded(RoutePoint departure, RoutePoint nextTarget)
    {
        if (!actor || !nextTarget) yield break;

        // From leaf: always U-turn
        if (departure && departure.pointType == RoutePointType.Leaf)
        {
            yield return PlayTurnAndWait("UTurn");
            yield break;
        }

        // From sleep: check which sleep point and which next point
        if (departure && departure.pointType == RoutePointType.Sleep)
        {
            if (nextTarget.pointType != RoutePointType.Turn) yield break;

            string sleepName = departure.name;
            string turnName = nextTarget.name;

            bool needUTurn = false;

            // SleepPointA �� LeafC, D, E
            if (sleepName.Contains("SleepPointA"))
            {
                //Debug.Log("DoUTurnIfNeeded!!");
                if (turnName == "TurnPointA")
                {
                    needUTurn = true;
                }
            }
            // SleepPointB �� LeafA, B, C, D
            else if (sleepName.Contains("SleepPointB"))
            {
                if (turnName == "TurnPointC")
                {
                    needUTurn = true;
                }
            }

            if (needUTurn)
            {
                yield return PlayTurnAndWait("UTurn");
            }
        }
    }

    // ===== Play Turn Animation and Wait =====
    private IEnumerator PlayTurnAndWait(string triggerName)
    {
        if (!anim) yield break;

        Debug.Log($"PlayTurnAndWait: triggering '{triggerName}'");

        anim.SetBool(HashIsMoving, false);
        anim.ResetTrigger(triggerName);
        anim.SetTrigger(triggerName);

        // Wait until SMB sets isTurningByAnim = true
        float guard = 0.5f;
        float t = 0f;
        while (!isTurningByAnim && (t += Time.deltaTime) < guard)
            yield return null;

        // Wait until SMB sets it back to false
        while (isTurningByAnim)
            yield return null;
    }

    // ===== Decide Turn Trigger =====
    private string DecideTurnTrigger(RoutePoint currentTurn, RoutePoint next)
    {
        if (!currentTurn || !next) return null;

        string cur = currentTurn.name;
        string nxt = next.name;

        if (cur == "TurnPointA") 
        { if (nxt == "TurnPointBa") return "AtoB"; return "AtoC"; }

        if (cur == "TurnPointB") 
        { if (nxt == "LeafPointE") return "BtoC"; return "BtoA"; }

        if (cur == "TurnPointC")
        { if (nxt == "TurnPointBa") return "CtoB"; return "CtoA"; }

        if (cur.Contains("a")) 
        { if (nxt == "LeafPointA" || nxt == "LeafPointC") return "aTOb"; return "aTOc"; }
        
        if (cur.Contains("b"))
        { if (nxt == "LeafPointB" || nxt == "LeafPointD") return "bTOc"; return "bTOa"; }
        
        if (cur.Contains("c"))
        { if (nxt == "LeafPointA" || nxt == "LeafPointC") return "cTOb"; return "cTOa"; }

        return null;
    }

    // ===== Determine Cluster from Name =====
    //private char ClusterOf(RoutePoint p)
    //{
    //    string n = p.name;

    //    if (n == "TurnPointA") return 'A';
    //    if (n == "TurnPointB") return 'B';
    //    if (n == "TurnPointC") return 'C';

    //    if (n == "TurnPointAa") return 'a';
    //    if (n == "TurnPointAb") return 'b';
    //    if (n == "TurnPointAc") return 'c';
    //    if (n == "TurnPointBa") return 'a';
    //    if (n == "TurnPointBb") return 'b';
    //    if (n == "TurnPointBc") return 'c';
    //    //if (n.Contains("TurnPointBc")) return 'c';

    //    return '?';
    //}

    // ===== Called from TurnMotionSMB =====
    public void SetTurningByAnim(bool v)
    {
        isTurningByAnim = v;
    }

    public void ApplyRotation(Quaternion rotation)
    {
        if (actor != null)
            actor.rotation = rotation;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
