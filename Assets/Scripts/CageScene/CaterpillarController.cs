using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaterpillarController : MonoBehaviour
{
    // ===== References =====
    public BranchStatus branchStatus;
    public CaterpillarStatus caterpillarStatus;

    // start node of the day (SleepPointA / SleepPointB)
    public RoutePoint currentPoint;

    [Header("Modules")]
    [SerializeField] private CaterpillarNavigator navigator;
    [SerializeField] private CaterpillarMover mover;

    [Header("Internal (debug)")]
    public List<string> todayTargets = new List<string>();
    public List<List<RoutePoint>> todayRoutes = new List<List<RoutePoint>>();

    private void Awake()
    {
        // auto assign if left empty
        if (!navigator) navigator = GetComponent<CaterpillarNavigator>();
        if (!mover) mover = GetComponent<CaterpillarMover>();
    }

    /// <summary>
    /// Called when the player feeds the caterpillar.
    /// Plans todayÅfs route and starts moving.
    /// </summary>
    public void OnFeed()
    {
        if (branchStatus == null || caterpillarStatus == null)
        {
            Debug.LogWarning("[CaterpillarController] BranchStatus or CaterpillarStatus is missing.");
            return;
        }
        if (navigator == null || mover == null)
        {
            Debug.LogWarning("[CaterpillarController] Navigator or Mover is missing.");
            return;
        }

        // 1) reset leaves
        branchStatus.SetAllFresh();

        // 2) how many leaves today
        int needCount = caterpillarStatus.NeededToday;

        // 3) plan routes by navigator
        RoutePoint sleepA = null;
        RoutePoint sleepB = null;
        if (branchStatus.sleepPoints != null)
        {
            if (branchStatus.sleepPoints.Length > 0) sleepA = branchStatus.sleepPoints[0];
            if (branchStatus.sleepPoints.Length > 1) sleepB = branchStatus.sleepPoints[1];
        }

        var plan = navigator.PlanRoutesForToday(
            currentPoint,
            needCount,
            branchStatus,
            sleepA, sleepB);

        if (plan == null || plan.routes == null || plan.routes.Count == 0)
        {
            Debug.LogWarning("[CaterpillarController] Navigator returned empty plan.");
            return;
        }

        //todayTargets = plan.targets;
        //todayRoutes = plan.routes;

        Debug.Log("[CaterpillarController] Targets: " + string.Join("Å®", todayTargets));

        // 4) let mover execute the plan
        mover.RunPlan(plan);
    }

    // ===== Debug helpers =====
    [ContextMenu("DEBUG/OnFeed")]
    private void _DBG_OnFeed()
    {
        OnFeed();
    }

    [ContextMenu("DEBUG/RunLastPlan")]
    private void _DBG_RunLastPlan()
    {
        if (mover == null) mover = GetComponent<CaterpillarMover>();

        if (todayRoutes == null || todayRoutes.Count == 0)
        {
            Debug.LogWarning("[CaterpillarController] No stored plan to run.");
            return;
        }

        //mover.RunPlannedRoutes(todayRoutes, todayTargets, currentPoint);
        var plan = new CaterpillarNavigator.PlanResult();
        plan.targets = todayTargets;
        plan.routes = todayRoutes;
        mover.RunPlan(plan);
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
