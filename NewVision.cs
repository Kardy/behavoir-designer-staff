using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
public class NewVision : Conditional
{
    public SharedStringList targetTag;

   

    public SharedTransform visionPoint;
    public SharedBool secectClosestTarget;

    public SharedFloat updateSpeed;
    public LayerMask objectLayerMask;
    public SharedFloat fieldOfViev;
    public SharedFloat minDistanceToDetect;

    public SharedFloat maxDistanceToDetect;


    public SharedFloat lostTargetDistance;
    public SharedFloat LostTargetDelay;
    public SharedFloat changeTargetDelay;

    public SharedGameObject currentTarget;

    //дополнительные цели из стороннего источника
    public SharedGameObjectList additionalTargets;

    private List<GameObject> potentialTargets = new List<GameObject>();
    private List<GameObject> sucsessfulTargets = new List<GameObject>();


    float serchDelay = 0;
    float cTargetDelay = 0;
    float lostTargetDelay = 0;
    GameObject targetCandidat = null;

    public override TaskStatus OnUpdate()
    {
       

        serchDelay += Time.deltaTime;
        cTargetDelay += Time.deltaTime;


        if (serchDelay > updateSpeed.Value)
        {

            // create potential target list

            Collider[] hitColliders = Physics.OverlapSphere(visionPoint.Value.position, maxDistanceToDetect.Value, objectLayerMask);
            potentialTargets.Clear();
            sucsessfulTargets.Clear();
            if (hitColliders.Length > 0) {
                for (int i = 0; i < hitColliders.Length; i++)
                {
                    for (int y = 0; y < targetTag.Value.Count; y++)
                    {
                        if (hitColliders[i].gameObject.tag == targetTag.Value[y])
                        {

                            potentialTargets.Add(hitColliders[i].gameObject);

                        }
                    }
                }
            }

            if (potentialTargets.Count > 0)
            {
                for (int i = 0; i < potentialTargets.Count; i++)
                {
                   // Debug.Log(potentialTargets[i].name);
                    //ли цель достаточно близко - ее видно автоматически 
                    if (Vector3.Distance(visionPoint.Value.position, potentialTargets[i].transform.position) < minDistanceToDetect.Value)
                    {
                        sucsessfulTargets.Add(potentialTargets[i]);

                    }
                    RaycastHit hit;
                    //для остальных целей нужна проверка рейкастом
                    if (Physics.Raycast(visionPoint.Value.position, potentialTargets[i].transform.position- visionPoint.Value.position, out hit, Mathf.Infinity, objectLayerMask)) {

                        //Vector3 dir = hit.point - visionPoint.Value.position;
                        //Debug.DrawRay(visionPoint.Value.position, dir, Color.green);
                        Debug.DrawLine(hit.point, hit.point + Vector3.up);
                       // Debug.Log(hit.transform.gameObject.name);
                        if (hit.transform.gameObject == potentialTargets[i])
                        {
                            // и проверка угла поля зрения

                            Vector3 targetDir =
                                
                                new Vector3 (potentialTargets[i].transform.position.x , 0 , potentialTargets[i].transform.position.z)

                                - new Vector3 (transform.position.x, 0, transform.position.z);

                            Vector3 forward = transform.forward;

                            float angle = Vector3.Angle(targetDir, forward);

                            if (angle < fieldOfViev.Value || potentialTargets[i] == currentTarget.Value) { 
                            sucsessfulTargets.Add(potentialTargets[i]);
                            }

                        }
                    }
                }
            }



            //проверяем список доступных целей на предмет того какая из них самая близкая
            targetCandidat = null;
            if (sucsessfulTargets.Count > 0)
            {
                float dist = maxDistanceToDetect.Value + lostTargetDistance.Value;
                foreach (GameObject target in sucsessfulTargets)
                {
                    if (Vector3.Distance(visionPoint.Value.position, target.transform.position) < dist)
                    {
                        dist = Vector3.Distance(visionPoint.Value.position, target.transform.position);
                        targetCandidat = target;

                    }
                }
                if (currentTarget.Value == null)
                {
                    currentTarget.Value = targetCandidat;
                }

                if (cTargetDelay > changeTargetDelay.Value)
                {
                    currentTarget.Value = targetCandidat;

                }
                else
                {
                    if (sucsessfulTargets.Contains(currentTarget.Value))
                    {

                        targetCandidat = currentTarget.Value;

                    }

                }



            }

            serchDelay = 0;
        }



        //проверяем не спряталась ли цель 
        if (currentTarget.Value != null) {

            if (targetCandidat == null)
            {
                Debug.Log("cant see");
               lostTargetDelay += Time.deltaTime;
            }
            else
            {
                lostTargetDelay = 0;
            }


        }

        if (lostTargetDelay > LostTargetDelay.Value)
        {
            currentTarget.Value = null;

        }

        //проверяем не убежала ли цель
        if (currentTarget.Value != null)
        {
            if(Vector3.Distance(visionPoint.Value.position, currentTarget.Value.transform.position) > lostTargetDistance.Value + maxDistanceToDetect.Value)
            {
                currentTarget.Value = null;
            }

        }



            if (currentTarget.Value != null) { 
		    return TaskStatus.Success;
            }
             return TaskStatus.Failure;
    }

    public override void OnReset()
    {
      
    }
}