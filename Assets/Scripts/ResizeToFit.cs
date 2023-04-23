using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResizeToFit : MonoBehaviour
{
    public GameObject container; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float width = container.GetComponent<RectTransform>().rect.width;
        float new_width = width/2; 
        if(new_width!=0) {
            Vector2 newSize = new Vector2(new_width, new_width); 
            container.GetComponent<GridLayoutGroup>().cellSize = newSize; 
        }
    }
}
