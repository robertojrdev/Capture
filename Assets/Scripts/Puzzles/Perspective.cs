using UnityEngine;

[System.Serializable]
public class Perspective : Puzzle
{
    public Vector3 lookPosition;
    [SerializeField] private float lookRange = 1;
    public Vector3 targetPosition;
    [SerializeField] private float targetAngleOfView = 45;

    private bool isInPuzzle = false;

    public override bool IsInPuzzle(Transform camera)
    {
        float dist = Vector3.Distance(camera.position, lookPosition + transform.position);
        if (dist <= lookRange)
        {
            isInPuzzle = true;
            return true;
        }
        else
        {
            isInPuzzle = false;
            return false;
        }
    }

    private void Solve()
    {

    }

    public float Angle(Vector3 target)
    {
        Vector3 direction = (target - GameCamera.instance.transform.position).normalized;
        float angle = Vector3.Angle(direction, GameCamera.instance.transform.forward);
        return angle;
    }

    public float FindRadius(float dist, float angle) //cateto oposto
    {
        float tan = Mathf.Tan(Mathf.Deg2Rad * angle);
        float radius = tan * dist;
        return radius;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + lookPosition, lookRange);

        float distance = Vector3.Distance(lookPosition, targetPosition);

        float radius = FindRadius(targetAngleOfView, distance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + targetPosition, radius);
    }

    public override bool TryToSolve()
    {
        if (!isInPuzzle || IsSolved)
            return false;

        float dotProduct = Angle(transform.position + targetPosition);
        if (dotProduct < targetAngleOfView)
        {
            IsSolved = true;
            if (!UseReceiver)
                onSolve.Invoke();
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void PutInReceiver()
    {
        if(UseReceiver && IsSolved)
		{
			onSolve.Invoke();
		}
    }
}
