using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientController : MonoBehaviour
{
    Rigidbody m_rigidbody;

    public string name;

    public float underwaterDrag = 3.0f;
    public float underwaterAngularDrag = 1.0f;
    public float airDrag = 0.0f;
    public float airAngularDrag = 0.05f;
    public float floatingPower = 100.0f;

    private float surfaceHeight = 0.0f;

    [SerializeField]
    private bool inCauldron = false;
    
    [SerializeField]
    private bool underwater = false;

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        surfaceHeight = CauldronController.instance.GetSurfaceHeight();

        UpdateState();
    }

    void FixedUpdate()
    {
        if (!inCauldron) return;

        float diff = transform.position.y - transform.localScale.y * 2.0f - surfaceHeight;
        if (diff < 0) {
            m_rigidbody.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(diff), transform.position, ForceMode.Force);
            if (!underwater) {
                underwater = true;
                UpdateState();
            }
        } else if (underwater) {
            underwater = false;
            UpdateState();
        }
    }

    private void UpdateState() {
        if (underwater) {
            m_rigidbody.drag = underwaterDrag;
            m_rigidbody.angularDrag = underwaterAngularDrag;
        } else {
            m_rigidbody.drag = airDrag;
            m_rigidbody.angularDrag = airAngularDrag;
        }
    }

    public void CauldronExploded() {
        inCauldron = false;
        underwater = false;
    }
    
    private void OnTriggerEnter(Collider collision)
    {
        if (!inCauldron && collision.gameObject.tag == "CauldronSurface")
        {
            inCauldron = true;
            CauldronController.instance.floatingObjects.Add(this.gameObject);
            CauldronController.instance.AddNewIngredient(name);
        }
    }
}
