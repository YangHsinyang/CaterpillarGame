using UnityEngine;

public enum RoutePointType { Leaf, Turn, Sleep }

public class RoutePoint : MonoBehaviour
{
    public RoutePointType pointType;
    public RoutePoint[] nextPoints;
}
