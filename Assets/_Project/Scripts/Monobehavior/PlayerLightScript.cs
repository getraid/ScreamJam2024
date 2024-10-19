using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightScript : MonoBehaviour
{
    [field: SerializeField] public Light PlayerLight { get; set; }
    [field: SerializeField] public float TimeUntilLightEnabledInSeconds { get; set; } = 10;
    [field: SerializeField] public float LightDisabledIntensity { get; set; } = 0f;
    [field: SerializeField] public float LightIntensity { get; set; } = 200f;

    private float timer = 0f;

    private bool toggled = false;

    private bool setEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerLight == null)
            throw new System.ArgumentNullException("PlayerLight missing");
        timer = 0f;
    }

    public void ToggleLight(bool setEnabled)
    {
        this.toggled = true;
        this.setEnabled = setEnabled;
    }
    
    public void HardSetLight(float value)
    {
        PlayerLight.intensity = value;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (toggled)
        {
            toggled = false;
            timer = 0f;
        }
        else
        {
            timer = Mathf.Clamp(timer + Time.deltaTime, 0f, TimeUntilLightEnabledInSeconds);            
        }
            

        if (timer != 0 && timer <= TimeUntilLightEnabledInSeconds)
        {
             if (setEnabled)
             {
                 if (PlayerLight.intensity <= LightIntensity)
                 {
                     PlayerLight.intensity = PlayerLight.intensity + EaseInExpo(Time.deltaTime) * 60;
                 }
                     
                 
                 // looks better imo, but doesn't work for negative
                 //PlayerLight.intensity = Mathf.Lerp(PlayerLight.intensity, LightIntensity, interp);
                 
                 
             }
             else
             {
                  if(PlayerLight.intensity >= LightDisabledIntensity)
                  {
                      PlayerLight.intensity = PlayerLight.intensity - EaseOutExpo(Time.deltaTime)* 60;
                  }
             }
             
             
        }
        
    }
    
    float EaseInExpo(float number) {
        return number == 0 ? 0 : Mathf.Pow(2, 10 * number - 10);
    }
    float EaseOutExpo(float number) 
    {
        return number == 1 ? 1 : 1 - Mathf.Pow(2, -10 * number);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("OutOfBounds"))
        {
            // disable the player light and toggle on afterwards.
            HardSetLight(0f);
            ToggleLight(true);
        }
        
        if (other.gameObject.CompareTag("SafeZone"))
        {
            ToggleLight(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
 
        if (other.gameObject.CompareTag("SafeZone"))
        {
            ToggleLight(true);
        }
    }
    
    
}
