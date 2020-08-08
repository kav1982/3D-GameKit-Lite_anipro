#if MAIN_CONTAIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public TransForm Target;

    public Vector3 PositionOffset;

    private TransForm m_TF;

    void Awake()
    {
    	m_TF = this.transform;
    }

    void LateUpdate()
    {
    	if(this.Target != null)
    	{
    		this.m_TF.localPosition = Target.position + PositionOffset;
    	}
    }
}

#endif
