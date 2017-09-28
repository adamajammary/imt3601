using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



/***************************************************************************
 * Used to create functions for Special Abilities that requires networking
 * 
 ***************************************************************************/


public class AbilityNetwork : NetworkBehaviour {

    private int modelChildNum = 1;






  

    ///////////// Functions for Stealth ability /////////////////

    public void useStealth(int modelChildNum, float activeTime, float transparancy)
    {
        this.modelChildNum = modelChildNum;
        CmdStealth(activeTime, transparancy);
    }
    
    [Command]
    public void CmdStealth(float activeTime, float transparancy)
    {
        StartCoroutine(stealth(activeTime,transparancy));
    }

    private IEnumerator stealth(float activeTime,float transparancy)
    {
        RpcSetTransparentFox(transparancy);
        yield return new WaitForSeconds(activeTime);
        RpcSetOrginalFox();
    }

    [ClientRpc]
    private void RpcSetTransparentFox(float transparancy)
    {
        Material[] materials;
        Color alfa;
      
        foreach (Transform child in this.transform.GetChild(modelChildNum))
        {
			if(child.gameObject.GetComponent<Renderer>() != null)
				materials = child.gameObject.GetComponent<Renderer>().materials;
			else if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
				materials = child.gameObject.GetComponent<SkinnedMeshRenderer>().materials;
			else
				continue;

			int count = 0;
            foreach (Material mat in materials)
            {
				alfa = mat.color;
                alfa.a = transparancy;
                materials[count++].SetColor("_Color", alfa);
            }
        }
    }

    [ClientRpc]
    public void RpcSetOrginalFox()
    {
        Material[] materials;
        Color alfa;
        float orginal = 1.0f;
        
        foreach (Transform child in this.transform.GetChild(modelChildNum)) {
			if (child.gameObject.GetComponent<Renderer>() != null)
				materials = child.gameObject.GetComponent<Renderer>().materials;
			else if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
				materials = child.gameObject.GetComponent<SkinnedMeshRenderer>().materials;
			else
				continue;
			int count = 0;
            foreach (Material mat in materials) {
				alfa = mat.color;
                alfa.a = orginal;
                materials[count++].SetColor("_Color", alfa);
			}
        }
    }
    /////////////////////////////////////////////////////////////////
}
