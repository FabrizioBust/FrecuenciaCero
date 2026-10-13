using UnityEngine;

namespace FrecuenciaCero
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class ForestFogController : MonoBehaviour
    {
        [Header("Aspecto general")]
        [SerializeField, InspectorName("Densidad general"), Range(0f, 2f), Tooltip("0 elimina la bruma. 0.85 es el valor inicial sutil.")]
        private float generalDensity = .85f;
        [SerializeField, InspectorName("Velocidad de movimiento"), Min(0f), Tooltip("Unidades de mundo por segundo; no depende de la camara.")]
        private float movementSpeed = .028f;
        [SerializeField, InspectorName("Color de niebla")] private Color fogColor = new Color(.64f, .74f, .79f, 1f);
        [SerializeField, InspectorName("Transparencia"), Range(0f, 1f), Tooltip("1 = completamente transparente. Reducir este valor hace la niebla mas visible.")]
        private float transparency = .88f;
        [SerializeField, InspectorName("Intensidad de variacion"), Range(0f, 1f)] private float variationIntensity = .55f;

        [Header("Dos capas independientes")]
        [SerializeField, InspectorName("Intensidad capa A"), Range(0f, 2f)] private float ambientStrength = .7f;
        [SerializeField, InspectorName("Intensidad capa B"), Range(0f, 2f)] private float bankStrength = .55f;
        [SerializeField, InspectorName("Cantidad de bancos"), Range(0, 24)] private int bankCount = 12;
        [SerializeField, InspectorName("Tinte de bancos")] private Color bankTint = new Color(.65f, .74f, .69f, 1f);
        [SerializeField, InspectorName("Area de bancos")] private Rect bankArea = new Rect(-32f, -24f, 64f, 48f);
        [SerializeField, Tooltip("Usar el río/sendero del retén. Desactivado en la zona del generador.")]
        private bool useExteriorDensity = true;
        [SerializeField, Tooltip("Usar el sendero y los claros del bosque abandonado, sin alterar la niebla del retén.")]
        private bool useAbandonedForestDensity;
        [SerializeField] private bool useCabinTrailDensity;

        [Header("Referencias preparadas en la escena")]
        [SerializeField] private SpriteRenderer ambient = null;
        [SerializeField] private SpriteRenderer[] bankPool = null;
        private sealed class Bank
        {
            public Vector2 origin, direction;
            public float age, duration, speed, opacity, width, height, angle;
            public int cycle;
        }
        private Bank[] banks;
        private Vector3 ambientOrigin;
        private float driftClock;
        private bool initialized;

        private void OnEnable()
        {
            initialized = false;
            Initialize();
            if (initialized) Apply(0f);
        }
        private void OnDisable()
        {
            if (ambient != null) ambient.enabled = false;
            if (bankPool != null)
                foreach (SpriteRenderer renderer in bankPool)
                    if (renderer != null) renderer.enabled = false;
        }
        private void OnValidate()
        {
            generalDensity = Mathf.Clamp(generalDensity, 0f, 2f);
            movementSpeed = Mathf.Max(0f, movementSpeed);
            bankCount = Mathf.Clamp(bankCount, 0, 24);
            bankArea.width = Mathf.Max(1f, bankArea.width);
            bankArea.height = Mathf.Max(1f, bankArea.height);
        }
        private void Initialize()
        {
            if (ambient == null || bankPool == null || bankPool.Length == 0) return;
            // Pool objects are serialized, owned by this scene and never created/destroyed each cycle.
            ambientOrigin = Vector3.zero;
            banks = new Bank[bankPool.Length];
            for (int i = 0; i < banks.Length; i++)
            {
                banks[i] = new Bank();
                ResetBank(i);
                banks[i].age = banks[i].duration * RandomValue(i, 71);
            }
            driftClock = 0;
            initialized = true;
        }
        private static float RandomValue(int index, int salt) => (float)ForestFogField.Hash(index + 97, salt + 53);
        private void ResetBank(int index)
        {
            Bank bank = banks[index];
            int seed = index + bank.cycle * 37;
            float x = Mathf.Lerp(bankArea.xMin, bankArea.xMax, RandomValue(seed, 3));
            float y = Mathf.Lerp(bankArea.yMin, bankArea.yMax, RandomValue(seed, 7));
            // A third of the pool drifts near the river, spread along its length.
            if (useExteriorDensity && index % 3 == 0)
            {
                y = Mathf.Lerp(-17f, 17f, RandomValue(seed, 11));
                x = (float)OrganicTerrainField.RiverCenter(y) + Mathf.Lerp(-3f, 3f, RandomValue(seed, 13));
            }
            bank.origin = new Vector2(x, y);
            float horizontal = RandomValue(seed, 17) < .28f ? -1f : 1f;
            bank.direction = new Vector2(horizontal, Mathf.Lerp(-.32f, .32f, RandomValue(seed, 19))).normalized;
            bank.speed = Mathf.Lerp(.55f, 1.35f, RandomValue(seed, 23));
            bank.duration = Mathf.Lerp(130f, 240f, RandomValue(seed, 29));
            bank.opacity = Mathf.Lerp(.75f, 1.1f, RandomValue(seed, 31));
            bank.width = Mathf.Lerp(10f, 16f, RandomValue(seed, 37));
            bank.height = Mathf.Lerp(4.5f, 8f, RandomValue(seed, 41));
            bank.angle = Mathf.Lerp(-11f, 11f, RandomValue(seed, 43));
            if (bankPool[index] != null)
            {
                bankPool[index].flipX = RandomValue(seed, 47) > .5f;
                bankPool[index].flipY = RandomValue(seed, 49) > .5f;
            }
            bank.age = 0;
        }
        private void Update()
        {
            if (!initialized) Initialize();
            if (initialized) Apply(Application.isPlaying ? Time.deltaTime : 0f);
        }
        private void Apply(float delta)
        {
            float opacity = Mathf.Clamp01(1f - transparency) * generalDensity;
            driftClock += delta * movementSpeed;
            ambient.enabled = opacity > 0f && ambientStrength > 0f;
            ambient.transform.localPosition = ambientOrigin + new Vector3(
                .28f * Mathf.Sin(driftClock * 1.8f), .18f * Mathf.Sin(driftClock * 1.3f), 0f);
            float breathing = 1f + variationIntensity * .08f * Mathf.Sin(driftClock * .7f);
            Color tint = fogColor;
            tint.a *= Mathf.Min(.18f, opacity * ambientStrength * breathing);
            ambient.color = tint;

            for (int i = 0; i < banks.Length; i++)
            {
                SpriteRenderer renderer = bankPool[i];
                if (renderer == null) continue;
                Bank bank = banks[i];
                bank.age += delta;
                if (bank.age >= bank.duration)
                {
                    bank.cycle++;
                    ResetBank(i); // The life envelope is already zero at this invisible relocation.
                }
                renderer.enabled = i < bankCount && opacity > 0f && bankStrength > 0f;
                Vector2 position = bank.origin + bank.direction * (bank.age * movementSpeed * bank.speed);
                renderer.transform.localPosition = new Vector3(position.x, position.y, 0f);
                renderer.transform.localScale = new Vector3(bank.width / 8f, bank.height / 4f, 1f);
                renderer.transform.localRotation = Quaternion.Euler(0f, 0f, bank.angle);
                float envelope = (float)ForestFogField.LifeFade(bank.age / bank.duration);
                Vector3 world = renderer.transform.position;
                float zone = useCabinTrailDensity ? CabinTrailField.FogDensity(world) : useAbandonedForestDensity ? AbandonedForestField.FogDensity(world) : useExteriorDensity ? (float)ForestFogField.ZoneDensity(world.x, world.y) :
                    Mathf.Lerp(.35f, 1f, Mathf.SmoothStep(0f, 1f, ((float)GeneratorTerrainField.PathDistance(world.x, world.y) - .4f) / 1.7f));
                float variation = Mathf.Lerp(1f, bank.opacity, variationIntensity);
                tint = Color.Lerp(fogColor, bankTint, .45f);
                tint.a *= Mathf.Min(.18f, opacity * bankStrength * variation * zone * envelope);
                renderer.color = tint;
            }
        }
    }
}
