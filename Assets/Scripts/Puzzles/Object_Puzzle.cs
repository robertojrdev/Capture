using UnityEngine;

public class Object_Puzzle : Puzzle
{
	public float maxDistance = 60;

    public override bool IsInPuzzle(Transform player)
    {
		float dist = Vector3.Distance(player.transform.position, transform.position);
        if(dist > maxDistance)
			return false;

		Vector3 direction = transform.position - GameCamera.instance.transform.position;
		float dot = Vector3.Dot(GameCamera.instance.transform.forward, direction.normalized);
		
		if(dot > 0.85f)
			return true;
		else
			return false;
    }

    public override void PutInReceiver()
    {
        if(UseReceiver && IsSolved)
		{
			onSolve.Invoke();
		}
    }

    public override bool TryToSolve()
    {
        if(IsSolved)
            return false;
    
        IsSolved = true;
        if(!UseReceiver)
            onSolve.Invoke();
        return true;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
