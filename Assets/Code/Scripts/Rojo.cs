using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static BehaviourPlus;
using static Cuadrado;

public class Rojo : MonoBehaviour_UU, IUpdatable
{
    [SerializeField] private float speed = 5f;
    private Vector2Int moveDir;
    private Vector3 targetPos;
    private float remainingDistance;
    private Vector2Int stopPos;
    private TileBase stopTile;

    #region OLD
    private int contactoMorado; //OnExit ocurre antes que OnEnter
    #endregion
    void OnDestroy() => gameManager.MoveRojo -= Move;

    void Start()
    {
        gameManager.MoveRojo += Move;
        gameManager.RegisterRojo();
        stopPos = (Vector2Int)gameManager.tilemap.WorldToCell(transform.position);
        gameManager.tilemap.SetTile(PlayerTile.StartPos, null); //todo
        ExperimentManager.Instance.UnpauseSession();
    }

    private void Move(Vector2Int dir)
    {
        moveDir = dir;
        CalculateMovement();
        if (stopTile == null) remainingDistance = int.MaxValue;
        else
        {
            targetPos = gameManager.tilemap.GetCellCenterWorld((Vector3Int)stopPos);
            remainingDistance = Vector3.Distance(transform.position, targetPos);
        }
    }

    private void CalculateMovement()
    {
        Vector2Int pos = stopPos;
        while (true)
        {
            Vector2Int next = pos + moveDir;
            if (!gameManager.tilemap.cellBounds.Contains((Vector3Int)next))
            {
                stopTile = null;
                return;
            }
            TileBase tile = gameManager.tilemap.GetTile((Vector3Int)next);
            if (tile != null && tile is not PlayerTile)
            {
                stopTile = tile;
                stopPos = pos;
                return;
            }
            pos = next;
        }
    }

    public void OnUpdate()
    {
        if (moveDir == Vector2Int.zero) return;
        float step = speed * Time.deltaTime;
        if (step >= remainingDistance)
        {
            transform.position = targetPos;
            FinishMovement();
        }
        else
        {
            transform.position += (Vector3)((Vector2)moveDir * step);
            remainingDistance -= step;
        }
    }

    private void FinishMovement()
    {
        //transform.position -= (Vector3)((Vector2)dir * 0.093f);
        //Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
        //Vector3Int pos = tilemap.WorldToCell(collision.contacts[0].point + (Vector2)dir * 0.05f);
        //TileBase tile = tilemap.GetTile(pos);
        Cuadrado c = gameManager.cuadrados.First(c => c.tile == stopTile);
        switch (c.accion)
        {
            case Accion.Detener:
                Stop();
                break;
            case Accion.Empujar:
                transform.position -= (Vector3)((Vector2)moveDir * 1.1f);
                Stop();
                break;
            case Accion.CambiarDireccion:
                if (moveDir == Vector2Int.up) Stop(); //4 direcciones
                else Move(Vector2Int.up);
                break;
            case Accion.Morir:
                Core.ReloadScene();
                return;
            case Accion.Finalizar:
                enabled = false;
                Core.Win();
                return;
        }
        if (c.finUsos is not FinUsos.Null)
        {
            c.nUsos--;
            if (c.nUsos <= 0)
            {
                switch (c.finUsos)
                {
                    case FinUsos.Desaparecer:
                        gameManager.tilemap.SetTile((Vector3Int)stopPos, null);
                        break;
                }
            }
        }
    }

    #region OLD
    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Morado":
                if (contactoMorado == 0) StartCoroutine(ContactoMorado(collision));
                contactoMorado--;
                break;
            case "Celeste Falso":
                Destroy(collision.gameObject);
                gameManager.CelesteFalsoCollected();
                break;
            case "Rosado":
                StartCoroutine(JumpRosado());
                break;
            case "Amarillo":
                StartCoroutine(ContactoAmarillo(collision));
                break;
            case "Verde Invertir":
                StartCoroutine(ContactoVerdeInvertir(collision));
                break;
        }
    }

    private IEnumerator JumpRosado()
    {
        //col.enabled = false;
        transform.DOScale(2, 0.15f);
        yield return new WaitForSeconds(0.15f);
        transform.DOScale(1, 0.15f);
        yield return new WaitForSeconds(0.15f);
        //col.enabled = true;
    }

    private IEnumerator ContactoMorado(Collider2D collision)
    {
        contactoMorado = 4;
        yield return new WaitUntil(() => Vector2.Distance(transform.position, collision.transform.position) < 0.05f);
        transform.position = gameManager.GetTeleportNewPos(collision.name);
    }

    private IEnumerator ContactoAmarillo(Collider2D collision)
    {
        yield return new WaitUntil(() => Vector2.Distance(transform.position, collision.transform.position) < 0.05f);
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        Stop();
    }

    private IEnumerator ContactoVerdeInvertir(Collider2D collision)
    {
        yield return new WaitUntil(() => Vector2.Distance(transform.position, collision.transform.position) < 0.05f);
        gameManager.InvertVerde();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Morado":
                contactoMorado--;
                break;
            case "CreaNegro":
                Destroy(collision.gameObject);
                Instantiate(gameManager.prefabNegro, collision.transform.position, Quaternion.identity);
                break;
        }
    }
    #endregion

    private void Stop()
    {
        moveDir = Vector2Int.zero;
        gameManager.RojoQuieto();
    }

    void OnBecameInvisible()
    {
        if (!Core.isLoading) Core.ReloadScene();
    }

}

/* Sprites: Negro X, Azules direccionales
 * Botón Azul
 * Bonus
 * Botón Negro-Naranja
 * Celeste
 * Marrón, Marrón direccionales, Marrón invertir, Marrón invertir X, Marrón reiniciar, Marrón reiniciar X => Al moverse, comenzar inactivas por si justo están sobre Rojo
 * Powerup Negro-Negro X
 * Flechas direccionales
 * Gris, Gris invertir, Gris invertir X, Gris check, Gris check X, Gris equis, Gris equis X => Usos antes de desaparecer
 * Verde prendido, Verde apagado, Verde invertir, Verde invertir X
 * Crea Negro X => Veces que Rojo debe pasarle encima, Cuadrado a crear
 * 
 * Rosado
 * Blanco
 */