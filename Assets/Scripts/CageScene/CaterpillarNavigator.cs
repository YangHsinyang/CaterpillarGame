using System.Collections.Generic;
using UnityEngine;

public class CaterpillarNavigator : MonoBehaviour
{
    public List<string> ShuffleAndTake(List<string> ids, int n)
    {
        if (ids == null) return new List<string>();
        for (int i = ids.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = ids[i];
            ids[i] = ids[j];
            ids[j] = temp;
        }
        if (n < 0) n = 0;
        if (n > ids.Count) n = ids.Count;
        var result = ids.GetRange(0, n);
        Debug.Log($"[ShuffleAndTake] Result: [{string.Join(", ", result)}]");
        return result;
    }

    public List<string> DefaultLeafIdList()
    {
        return new List<string> { "A", "B", "C", "D", "E" };
    }

    public List<RoutePoint> Bfs(RoutePoint start, RoutePoint goal)
    {
        if (start == null || goal == null) return new List<RoutePoint>();

        var q = new Queue<RoutePoint>();
        var visited = new HashSet<RoutePoint>();
        var parent = new Dictionary<RoutePoint, RoutePoint>();

        q.Enqueue(start);
        visited.Add(start);
        parent[start] = null;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == goal)
                return ReconstructPath(parent, start, goal);

            if (cur.nextPoints != null)
            {
                for (int i = 0; i < cur.nextPoints.Length; i++)
                {
                    var nxt = cur.nextPoints[i];
                    if (nxt == null) continue;
                    if (visited.Contains(nxt)) continue;

                    visited.Add(nxt);
                    parent[nxt] = cur;
                    q.Enqueue(nxt);
                }
            }
        }
        return new List<RoutePoint>(); // unreachabl
    }

    private List<RoutePoint> ReconstructPath(Dictionary<RoutePoint, RoutePoint> parent, RoutePoint start, RoutePoint goal)
    {
        var path = new List<RoutePoint>();
        var cur = goal;

        while (cur != null)
        {
            path.Add(cur);
            cur = parent[cur];
        }

        path.Reverse();
        return path;
    }

    public RoutePoint MapSleepPointByLeafId(string lastLeafId, RoutePoint sleepA, RoutePoint sleepB)
    {
        if (string.IsNullOrEmpty(lastLeafId))
            return sleepA ?? sleepB;

        // A/B -> sleepB
        if (lastLeafId == "A" ||  lastLeafId == "B")
            return sleepB ?? sleepA;

        // C/D/E -> sleepA
        return sleepA ?? sleepB;
    }

    public PlanResult PlanRoutesForToday(
        RoutePoint start,
        int needCount,
        BranchStatus branch,
        RoutePoint sleepA, RoutePoint sleepB)
    {
        var res = new PlanResult();

        // 1) Determine target leaf IDs (A..E) for today
        res.targets = ShuffleAndTake(DefaultLeafIdList(), needCount);

        // 2) Build legs start->leaf1->leaf2...
        var routes = new List<List<RoutePoint>>();
        var curStart = start;
        string lastOkLeafId = null;

        for (int i = 0; i < res.targets.Count; i++)
        {
            var leafId = res.targets[i];
            var leafPoint = branch.GetRoutePointById(leafId);

            var leg = (leafPoint != null) ? Bfs(curStart, leafPoint) : new List<RoutePoint>();
            routes.Add(leg);

            if (leafPoint != null && leg.Count > 0)
            {
                curStart = leafPoint;   // advance start only when reachable
                lastOkLeafId = leafId;  // remember last successful leaf id
            }
        }

        // 3) Append final leg to the mapped sleep point
        var keyId = lastOkLeafId ?? (res.targets.Count > 0 ? res.targets[res.targets.Count - 1] : null);
        var sleepGoal = MapSleepPointByLeafId(keyId, sleepA, sleepB);
        var toSleep = (sleepGoal != null) ? Bfs(curStart, sleepGoal) : new List<RoutePoint>();
        routes.Add(toSleep);

        res.routes = routes;
        return res;
    }

    public class PlanResult
    {
        public List<string> targets = new List<string>();
        public List<List<RoutePoint>> routes = new List<List<RoutePoint>>();
    }
}
