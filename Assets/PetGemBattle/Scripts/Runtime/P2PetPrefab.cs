using UnityEngine;
namespace PetGemBattle
{
    // Portable UI pet: editor preview, rebuilt as an event-driven layered rig at runtime.
    public sealed class P2PetPrefab : MonoBehaviour
    {
        public string sheet;
        public int species;
        public Element element;
        public bool facingRight=true;
        public ArticulatedPet Rig {get;private set;}
        private void Awake()
        {
            var preview=transform.Find("EditorPreview");
            if(preview!=null){preview.gameObject.SetActive(false);Destroy(preview.gameObject);}
            Rig=gameObject.AddComponent<ArticulatedPet>();
            Rig.Build(species,facingRight,P1Catalog.ElementColor(element),sheet);
        }
    }
}
