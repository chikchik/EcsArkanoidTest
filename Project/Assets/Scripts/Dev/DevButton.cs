using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DevButton : MonoBehaviour
{
    public Image Back;
    public TMP_Text Text;
    public Button Button;
    
    public DevButton setColor(Color c)
    {
        Back.color = c;
        return this;
    }

    public DevButton setName(string name)
    {
        gameObject.name = name;
        return this;
    }
    
    public DevButton setText(string tx)
    {
        Text.text = tx;
        Text.name = tx;
        return this;
    }
}
