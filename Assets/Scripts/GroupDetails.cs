using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GroupDetails : MonoBehaviour
{
    [SerializeField]
    private TMP_Text groupName;

    [SerializeField] 
    private TMP_Text members;
    
    
        
    // Start is called before the first frame update
    void Start()
    {
        LoadGroupDetails();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void LoadGroupDetails()
    {
        var groupNamePrefs = PlayerPrefs.GetString("SelectedGroupName", "Group Not Found");

        groupName.text = groupNamePrefs;
    }
}
