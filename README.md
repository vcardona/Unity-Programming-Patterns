# Unity-Programming-Patterns

### Un ejercicio de programación sobre como implementar patrones de programación en videojuegos.

En este tutorial se explican algunos de los patrones de programación en Unity con C#, esta es la explicación que da Wikipedia sobre que es un patrón de programación para los que no esten relacionados con el termino:

En ingenieria de software, un patrón de diseño de software es una solución reusable para un problema que ocurre comunmente en un contexto especifico en el diseño de software. No es un diseño final que puede ser transformado directamente en código o lenguaje maquina. Es una descripción o template sobre como podemos resolver un problema, el cual puede ser usado en diferentes situaciones. Los patrones de diseño son buenas practicas que un programador puedes usar par resolver un problema común cuando de diseña una aplicación o un sistema.

#### [01 - Command Pattern](#CommandPattern)
#### [02 - FlyWeight Pattern](#FlyweightPattern)
#### [03 - Observer Pattern](#ObserverPattern)
#### [04 - State Pattern](#StatePattern)
#### [05 - SubClass SandBox Pattern](#SubClassPattern)
#### [06 - Spatial Partition Pattern](#SpatialPattern)

<a name="CommandPattern"></a>
## 01 - Command Pattern

En programación orientada a objetos, el Command Pattern es un patrón de diseño de comportamiento en el cual un objeto es usado para encapsular toda la información necesaria para ejecutar una acción o activar algún evento en otro momento. Esta información incluye el nombre del método, el objeto que es propietario del método y los valores de los parametros del método.

Un ejemplo donde el Command Pattern es realmente útil, es cuando le damos la opción a nuestros usuarios de cambiar los controles del juego, en Unity podemos escribir una función similar a esta:
```csharp
if (Input.GetKeyDown(KeyCode.A))
{
	FireWeapon();
}
```
Que podemos hacer si el usuario quiera disparar su arma presionado la tecla U en lugar de la A? Para hacer que esto pase, debemos buscar la forma de reemplazar FireWeapon(); con algo más general, por esto es que necesitamos el Command Pattern. Iniciamos definiendo una clase base llamada Command.

```csharp
public abstract class Command
{
	public abstract void Execute();
}
```
Luego FireWeapon hereda de la clase base. Necesitamos una clase que no haga nada, de esta forma podemos hacer el Switch entre la U y la A cuando se dispare el arma.

```csharp
public class FireWeapon : Command
{
	public override void Execute()
	{
		FireTheWeapon();
	}
}
public class DoNothing : Command
{
	public override void Execute()
	{
	}
}
```
Como estaba antes nuestro código solo podiamos disparar nuestra arma presionando la tecla A, ahora facilmente podemos reemplazar FireWeapon(), con un Execute() que es más general presionando la tecla U. De esta forma haciendo uso de una interfaz podemos seleccionar una nueva configuración y en lugar de usar el Button A usamos el Button U, lo que hace mucho más facíl, que usar el FireWeapon al presionar una tecla determinada o fija.

```csharp
Command buttonU = new FireWeapon();
Command buttonA = new DoNothing();

if (Input.GetKeyDown(KeyCode.U))
{
	buttonU.Execute(); //Ahora podemos llamar el método FireTheWeapon()  en el método Execute() en la clase FireWeapon 
}
if (Input.GetKeyDown(KeyCode.A))
{
	buttonA.Execute(); //No hace nada por que el método Execute() en la clase DoNothing esta vacio.
}
```
Pero esto no es todo, Si estamos salvando los commands, estamos enviandolos a una lista, entonces podemos deshacer los commands. Digamos por ejemplo que tenemos una editor de niveles, en el cual estamos poniendo arboles. Luego podemos usar el Command Pattern para deshacer el ultimo o todos los comandos en el caso que ubiquemos alguno en una posición erronea.

Al usar la misma lista cuando deshacemos algo podemos crear una función que haga un replay, si recordamos la posición inicial, simplemente recorremos los commands que salvamos y Unity va a reproducir todo lo que hicimos. Esta es una buena forma de salvar la posición y la orientación en cada frame de todos nuestros objetos en la escena, luego repetir las posiciones haciendo un loop en la lista.

### El Command Pattern en Unity

La idea en este script es que tenemos un box gameobject, luego podemos mover la caja usando las teclas WASD, pero gracias al command pattern podemos deshacer los movimientos con la tecla Z, luego reproducir todos los movimientos desde el principio con la tecla R.

Lo que se necesita en la escena es una caja y un objeto vacio, agregamos el script InputHandler.cs al objeto vacio y agregamos la caja al espacio definido en el script. Con esto debe funcionar el ejemplo.

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CommandPattern
{
    public class InputHandler : MonoBehaviour
    {
        //The box we control with keys
        public Transform boxTrans;
        //The different keys we need
        private Command buttonW, buttonS, buttonA, buttonD, buttonB, buttonZ, buttonR;
        //Stores all commands for replay and undo
        public static List<Command> oldCommands = new List<Command>();
        //Box start position to know where replay begins
        private Vector3 boxStartPos;
        //To reset the coroutine
        private Coroutine replayCoroutine;
        //If we should start the replay
        public static bool shouldStartReplay;
        //So we cant press keys while replaying
        private bool isReplaying;


        void Start()
        {
            //Bind keys with commands
            buttonB = new DoNothing();
            buttonW = new MoveForward();
            buttonS = new MoveReverse();
            buttonA = new MoveLeft();
            buttonD = new MoveRight();
            buttonZ = new UndoCommand();
            buttonR = new ReplayCommand();

            boxStartPos = boxTrans.position;
        }



        void Update()
        {
            if (!isReplaying)
            {
                HandleInput();
            }

            StartReplay();
        }


        //Check if we press a key, if so do what the key is binded to 
        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                buttonA.Execute(boxTrans, buttonA);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                buttonB.Execute(boxTrans, buttonB);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                buttonD.Execute(boxTrans, buttonD);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                buttonR.Execute(boxTrans, buttonZ);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                buttonS.Execute(boxTrans, buttonS);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                buttonW.Execute(boxTrans, buttonW);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                buttonZ.Execute(boxTrans, buttonZ);
            }
        }


        //Checks if we should start the replay
        void StartReplay()
        {
            if (shouldStartReplay && oldCommands.Count > 0)
            {
                shouldStartReplay = false;

                //Stop the coroutine so it starts from the beginning
                if (replayCoroutine != null)
                {
                    StopCoroutine(replayCoroutine);
                }

                //Start the replay
                replayCoroutine = StartCoroutine(ReplayCommands(boxTrans));
            }
        }


        //The replay coroutine
        IEnumerator ReplayCommands(Transform boxTrans)
        {
            //So we can't move the box with keys while replaying
            isReplaying = true;
            
            //Move the box to the start position
            boxTrans.position = boxStartPos;

            for (int i = 0; i < oldCommands.Count; i++)
            {
                //Move the box with the current command
                oldCommands[i].Move(boxTrans);

                yield return new WaitForSeconds(0.3f);
            }

            //We can move the box again
            isReplaying = false;
        }
    }
}
```
Este scriipt recoge todos los comandos que necesitamos, es posible que sea necesario tener una clase por cada script cuando estamos creando nuestro propio command pattern.

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CommandPattern
{
    //The parent class
    public abstract class Command
    {
        //How far should the box move when we press a button
        protected float moveDistance = 1f;

        //Move and maybe save command
        public abstract void Execute(Transform boxTrans, Command command);

        //Undo an old command
        public virtual void Undo(Transform boxTrans) { }

        //Move the box
        public virtual void Move(Transform boxTrans) { }
    }


    //
    // Child classes
    //

    public class MoveForward : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);
            
            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.forward * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.forward * moveDistance);
        }
    }


    public class MoveReverse : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);

            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.forward * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.forward * moveDistance);
        }
    }


    public class MoveLeft : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);

            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.right * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.right * moveDistance);
        }
    }


    public class MoveRight : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Move the box
            Move(boxTrans);

            //Save the command
            InputHandler.oldCommands.Add(command);
        }

        //Undo an old command
        public override void Undo(Transform boxTrans)
        {
            boxTrans.Translate(-boxTrans.right * moveDistance);
        }

        //Move the box
        public override void Move(Transform boxTrans)
        {
            boxTrans.Translate(boxTrans.right * moveDistance);
        }
    }


    //For keys with no binding
    public class DoNothing : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            //Nothing will happen if we press this key
        }
    }


    //Undo one command
    public class UndoCommand : Command
    {
        //Called when we press a key
        public override void Execute(Transform boxTrans, Command command)
        {
            List<Command> oldCommands = InputHandler.oldCommands;

            if (oldCommands.Count > 0)
            {
                Command latestCommand = oldCommands[oldCommands.Count - 1];

                //Move the box with this command
                latestCommand.Undo(boxTrans);

                //Remove the command from the list
                oldCommands.RemoveAt(oldCommands.Count - 1);
            }
        }
    }


    //Replay all commands
    public class ReplayCommand : Command
    {
        public override void Execute(Transform boxTrans, Command command)
        {
            InputHandler.shouldStartReplay = true;
        }
    }
}
```
<a name="FlyweightPattern"></a>
## 02 - Flyweight Pattern

Un Patrón Flyweight es un objeto que minimiza el uso de la memoria, compartiendo la mayor cantidad de datos posibles con otros objetos similares, es una forma de usar objetos en gran cantidad, cuando una simple representación repetida utiliza una cantidad inaceptable de memoria.

La idea básica es que, si tenemos una gran cantidad de objetos en nuestro juego, existe una gran probabilidad que se pueda incrementar el rendimiento de nuestro juego, haciendo estos objetos mucho más livianos. Para que esto funcione nuestros objetos deben compartir la mayor cantidad de datos que son similares para todos los objetos. Si ellos comparten algún dato, entonces podemos crear el dato una sola vez y luego almacenar una referencia a este dato en el objeto. Puede sonar un poco complicado pero es algo sencillo.

Un ejemplo en el cual se puede aplicar el uso de este patrons es si estamos creando un nivel y agregamos árboles, cada árbol es igual que los otros, hay algo que comparten en común, tienen la misma maya y la misma textura, lo único que no comparten es la posición. 

Ahora veamos como implementar el patrón Flyweight, para este ejemplo utilizamos Aliens con miles de ojos, brazos y piernas, para esto necesitamos una clase alien que describa el alien.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlyweightPattern
{
    //Class that includes lists with position of body parts
    public class Alien
    {
        public List<Vector3> eyePositions;
        public List<Vector3> legPositions;
        public List<Vector3> armPositions;
    }
}
```
Creamos los aliens y generamos nuevas posiciones para cada parte del cuerpo de cada Alien.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Flyweight design pattern main class
namespace FlyweightPattern
{
    public class Flyweight : MonoBehaviour
    {
        //The list that stores all aliens
        List<Alien> allAliens = new List<Alien>();

        List<Vector3> eyePositions;
        List<Vector3> legPositions;
        List<Vector3> armPositions;


        void Start()
        {
            //List used when flyweight is enabled
            eyePositions = GetBodyPartPositions();
            legPositions = GetBodyPartPositions();
            armPositions = GetBodyPartPositions();

            //Create all aliens
            for (int i = 0; i < 10000; i++)
            {
                Alien newAlien = new Alien();

                //Add eyes and leg positions

                //Sin el flyweight
                newAlien.eyePositions = GetBodyPartPositions();
                newAlien.armPositions = GetBodyPartPositions();
                newAlien.legPositions = GetBodyPartPositions();

                //Con el flyweight
                //newAlien.eyePositions = eyePositions;
                //newAlien.armPositions = legPositions;
                //newAlien.legPositions = armPositions;

                allAliens.Add(newAlien);
            }
        }


        //Generate a list with body part positions
        List<Vector3> GetBodyPartPositions()
        {
            //Create a new list
            List<Vector3> bodyPartPositions = new List<Vector3>();

            //Add body part positions to the list
            for (int i = 0; i < 1000; i++)
            {
                bodyPartPositions.Add(new Vector3());
            }

            return bodyPartPositions;
        }
    }
}
```
Para ver lo que hace nuestro código y el patrón que estamos implementando, agregamos el Flyweight.cs a un objeto vacío, presionamos play en Unity y abrimos el profiler, luego vamos a la sección Memory, vamos a ver la siguiente imagen.


![](ProfilerSin.png)


La parte donde debemos poner atención es Mono, como podemos leer en este [link](https://docs.unity3d.com/Manual/ProfilerMemory.html), Unity explica que es el “total heap size and used heap size used by Managed Code - this memory is garbage collected” actualmente está más o menos en 484.1MB, este número es el que vamos a reducir usando el Flyweight Pattern.

Para agregar el Flyweight Pattern, asumimos que cada Alien tiene la misma posición para cada una de las partes de su cuerpo, puede que no sea realista, pero lo que estamos haciendo es una prueba sobre como funciona este patrón, para ver su funcionamiento comentamos la sección que dice “Sin el flyweight: y descomentamos la que dice “Con flyweight”, podemos ver el ejemplo en la siguiente imagen.

![](SinFlyweight.png)

Luego de hacer esto, ejecutamos de nuevo nuestro código en Unity, abrimos el profiler y vamos de nuevo a la sección Memory, revisamos de nuevo donde dice Mono, y podemos ver como se redujo considerablemente a 14.3 MB.

![](ProfilerCon.png)

<a name="ObserverPattern"></a>
## 03 - Observer Pattern

El patrón Observer es un patrón de diseño de software en el cual un objeto, llamado subject, mantiene una lista de sus dependientes, llamados observers, estos son notificados automáticamente cuando algún estado cambia, usualmente llamando alguno de sus métodos.

La idea básicamente es que podemos usar el observer pattern cuando necesitamos que muchos objetos reciban una actualización cuando otro objeto cambia. El observer pattern es actualmente la columna vertebral de muchos programas y aplicaciones, por eso es importante conocerlo.

Uno de los ejemplos es cuando tenemos logros o achievements en nuestro juego, esto puede ser un poco complicado para implementar si tenemos muchos logros y cada uno se desbloquea por medio de un comportamiento diferente, pero es mucho más fácil de implementar si conocemos el observer pattern.

Para ver el funcionamiento del Observer Pattern en Unity, tenemos tres boxes que van a saltar cuando una esfera esté cerca del centro del mapa.

![](ObserverPAttern.png)

Cada caja debe tener un rigidbody agregado para poder que salte, luego necesitamos un objeto vacío llamado _GameController que va a contener el Script GameController que va estar pendiente de todo.

El script del GameController se ve de la siguiente forma:

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern
{
    public class GameController : MonoBehaviour
    {
        public GameObject sphereObj;
        //The boxes that will jump
        public GameObject box1Obj;
        public GameObject box2Obj;
        public GameObject box3Obj;

        //Will send notifications that something has happened to whoever is interested
        Subject subject = new Subject();


        void Start()
        {
            //Create boxes that can observe events and give them an event to do
            Box box1 = new Box(box1Obj, new JumpLittle());
            Box box2 = new Box(box2Obj, new JumpMedium());
            Box box3 = new Box(box3Obj, new JumpHigh());

            //Add the boxes to the list of objects waiting for something to happen
            subject.AddObserver(box1);
            subject.AddObserver(box2);
            subject.AddObserver(box3);
        }


        void Update()
        {
            //The boxes should jump if the sphere is cose to origo
            Debug.Log((sphereObj.transform.position).magnitude);
            if ((sphereObj.transform.position).magnitude < 7f)
            {
                subject.Notify();
            }
        }
    }
}

```

El observer pattern básico se compone en dos partes:

* El Subject contiene la lista de todos los observers interesados en obtener información cuando algo sucede, cuando ocurre algún evento, este envía la información a todos los interesados.
* Los observers son los objetos interesados en hacer algo cuando el evento ocurre.

El siguiente script es la clase subject que envía la información a los observers:

### Subject Script
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern
{
    //Invokes the notificaton method
    public class Subject
    {
        //A list with observers that are waiting for something to happen
        List<Observer> observers = new List<Observer>();

        //Send notifications if something has happened
        public void Notify()
        {
            for (int i = 0; i < observers.Count; i++)
            {
                //Notify all observers even though some may not be interested in what has happened
                //Each observer should check if it is interested in this event
                observers[i].OnNotify();
            }
        }

        //Add observer to the list
        public void AddObserver(Observer observer)
        {
            observers.Add(observer);
            
        }

        //Remove observer from the list
        public void RemoveObserver(Observer observer)
        {
        }
    }
}

```
### Observer Script

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern
{
    //Wants to know when another object does something interesting 
    public abstract class Observer
    {
        public abstract void OnNotify();
    }

    public class Box : Observer
    {
        //The box gameobject which will do something
        GameObject boxObj;
        //What will happen when this box gets an event
        BoxEvents boxEvent;

        public Box(GameObject boxObj, BoxEvents boxEvent)
        {
            this.boxObj = boxObj;
            this.boxEvent = boxEvent;
        }

        //What the box will do if the event fits it (will always fit but you will probably change that on your own)
        public override void OnNotify()
        {
            Jump(boxEvent.GetJumpForce());
            
        }

        //The box will always jump in this case
        void Jump(float jumpForce)
        {
            //If the box is close to the ground
            if (boxObj.transform.position.y < 2f)
            {
                boxObj.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce);
                
                
            }
        }
    }
}
```
### BoxEvents Script
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern
{
    //Events
    public abstract class BoxEvents
    {
        public abstract float GetJumpForce();
    }


    public class JumpLittle : BoxEvents
    {
        public override float GetJumpForce()
        {
            return 30f;
        }
    }


    public class JumpMedium : BoxEvents
    {
        public override float GetJumpForce()
        {
            return 60f;
        }
    }


    public class JumpHigh : BoxEvents
    {
        public override float GetJumpForce()
        {
            return 90f;
        }
    }
}
```
Para configurar toda la escena y que funcione agregamos el script _GameController a un empty, luego agregamos al _GameController en el espacio de las variables definidas, la esfera y los tres cubos. Cada uno de los scripts que están en el ejemplo deben estar agregados en Unity, no se deben agregar a ninguno de los elementos en la escena, si vemos el código del _GameController el realiza esta tarea. 

Luego ejecutamos y movemos en la escena la esfera, hasta que su  magnitud sea menor de 7, para permitir que se muevan los cubos, podemos verificar en la consola el valor de la magnitud de la esfera para saber en que valor esta y poder activar el movimiento de los cubos.

![](Observergif.gif)

<a name="StatePattern"></a>
## 04 - State Pattern

El State Pattern, es una patrón de diseño de Software de comportamiento, este patrón es usado en programación para encapsular un comportamiento variable para el mismo objeto, basado en su estado interno.

La idea del patrón es que lo que estamos tratando de controlar tiene diferentes estados, si tenemos un carro este se puede manejar, frenar, reversar o puede esta parqueado, para escribir este comportamiento de un carro en código no es muy sencillo como se pueda pensar. Si el carro avanza al frente y frenamos, luego no queremos que reverse si el botón del freno y la reversa son el mismo. Qué sucede si estamos reversando y presionando el botón para ir adelante? Debemos frenar o acelerar? Que pasa si tenemos más de 4 estados diferentes? PAra esto necesitamos un state pattern que elimine todos los if else anidado y cada uno de esos estados con un montón de booleanos.

Lo primero que debemos preguntarnos es: Cuáles son todos los posibles estados o situaciones en las cuales el objeto se puede encontrar? La idea de dividir el comportamiento en diferentes estados en los que el objeto puede estar es llamado Finite State Machine o FSM. El objeto puede solamente estar en un estado a la vez, no podemos acelerar y presionar el freno al mismo tiempo.

Es muy común usar enums cuando estamos codificando una máquina de estados en C#, el siguiente código puede dar un ejemplo sobre nuestro carro.
```csharp
enum CarFSM
{
	Forward,
	Brake,
	Reverse,
	Park
}
//Default state
CarFSM carMode = CarFSM.Park; 

//And then you change the state with something like
if (carMode == CarFSM.Park && IsPressingGasPedal())
{
	carMode = CarFSM.Forward;
}
else if (carMode == CarFSM.Forward && IsPressingBrakePedal())
{
	carMode == CarFSM.Brake;
}
//...and so on
```

Para ilustrar el State Pattern vamos a crear dos criaturas, un Skeleton y un Creeper, el Skeleton atacará desde una distancia con un arco y una flecha, el Creeper atacará cuando está cerca, si el jugador está lejos de ambos enemigos, van a caminar en direcciones aleatorias esperando que su objetivo está cerca.

Para implementar el State Pattern vamos a crear lo siguiente:

* Un plano que va servir como terreno para los enemigos y el jugador.
* Una esfera que va ser el jugador que quieren eliminar los enemigos.
* Una caja verde que va a simbolizar al Creeper.
* Una caja blanca que simboliza el Skeleton.
* Un objeto vacío llamado _GameController.

La escena se debe ver de la siguiente forma:

![](ConfigStatePattern.png)

El Script del GameController se ve así:

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatePattern
{
    public class GameController : MonoBehaviour
    {
        public GameObject playerObj;
        public GameObject creeperObj;
        public GameObject skeletonObj;

        //A list that will hold all enemies
        List<Enemy> enemies = new List<Enemy>();


        void Start()
        {
            //Add the enemies we have
            enemies.Add(new Creeper(creeperObj.transform));
            enemies.Add(new Skeleton(skeletonObj.transform));
        }


        void Update()
        {
            //Update all enemies to see if they should change state and move/attack player
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].UpdateEnemy(playerObj.transform);
            }
        }
    }
}

```

Nuestro State Pattern consiste en una clase Enemy y dos clases hijas para cada enemy, la idea básica es primero actualizamos el estado del enemigo, que puede ser Attack, Flee, Stroll or Move, esta última en dirección al enemigo para atacar, todo esto verificando si puede cambiar entre un estado o el otro, dependiendo del estado en el cual se encuentre el enemigo. Si el enemigo está rondando y el jugador está a cierta distancia, entonces el enemigo iniciar la cacería del jugador.

La diferencia principal entre el Creeper y el Skeleton is que el Skeleton puede atacar desde una distancia, mientras que el Creeper debe estar muy cerca del jugador para poder atacar. 

La clase Enemy es así:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatePattern
{
    //The enemy base class
    public class Enemy
    {
        protected Transform enemyObj;

        //The different states the enemy can be in
        protected enum EnemyFSM
        {
            Attack,
            Flee,
            Stroll,
            MoveTowardsPlayer
        }


        //Update the enemy by giving it a new state
        public virtual void UpdateEnemy(Transform playerObj)
        {

        }


        //Do something based on a state
        protected void DoAction(Transform playerObj, EnemyFSM enemyMode)
        {
            float fleeSpeed = 10f;
            float strollSpeed = 1f;
            float attackSpeed = 5f;

            switch (enemyMode)
            {
                case EnemyFSM.Attack:
                    //Attack player
                    break;
                case EnemyFSM.Flee:
                    //Move away from player
                    //Look in the opposite direction
                    enemyObj.rotation = Quaternion.LookRotation(enemyObj.position - playerObj.position);
                    //Move
                    enemyObj.Translate(enemyObj.forward * fleeSpeed * Time.deltaTime);
                    break;
                case EnemyFSM.Stroll:
                    //Look at a random position
                    Vector3 randomPos = new Vector3(Random.Range(0f, 100f), 0f, Random.Range(0f, 100f));
                    enemyObj.rotation = Quaternion.LookRotation(enemyObj.position - randomPos);
                    //Move
                    enemyObj.Translate(enemyObj.forward * strollSpeed * Time.deltaTime);
                    break;
                case EnemyFSM.MoveTowardsPlayer:
                    //Look at the player
                    enemyObj.rotation = Quaternion.LookRotation(playerObj.position - enemyObj.position);
                    //Move
                    enemyObj.Translate(enemyObj.forward * attackSpeed * Time.deltaTime);
                    break;
            }
        }
    }
}

```

La clase Skeleton se ve así:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatePattern
{
    //The skeleton class
    public class Skeleton : Enemy
    {
        EnemyFSM skeletonMode = EnemyFSM.Stroll;

        float health = 100f;


        public Skeleton(Transform skeletonObj)
        {
            base.enemyObj = skeletonObj;
        }


        //Update the creeper's state
        public override void UpdateEnemy(Transform playerObj)
        {
            //The distance between the Creeper and the player
            float distance = (base.enemyObj.position - playerObj.position).magnitude;

            switch (skeletonMode)
            {
                case EnemyFSM.Attack:
                    if (health < 20f)
                    {
                        skeletonMode = EnemyFSM.Flee;
                    }
                    else if (distance > 6f)
                    {
                        skeletonMode = EnemyFSM.MoveTowardsPlayer;
                    }
                    break;
                case EnemyFSM.Flee:
                    if (health > 60f)
                    {
                        skeletonMode = EnemyFSM.Stroll;
                    }
                    break;
                case EnemyFSM.Stroll:
                    if (distance < 10f)
                    {
                        skeletonMode = EnemyFSM.MoveTowardsPlayer;
                    }
                    break;
                case EnemyFSM.MoveTowardsPlayer:
                    //The skeleton has bow and arrow so can attack from distance
                    if (distance < 5f)
                    {
                        skeletonMode = EnemyFSM.Attack;
                    }
                    else if (distance > 15f)
                    {
                        skeletonMode = EnemyFSM.Stroll;
                    }
                    break;
            }

            //Move the enemy based on a state
            DoAction(playerObj, skeletonMode);
        }
    }
}

```

La clase Creeper se ve así:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatePattern
{
    //The creeper class
    public class Creeper : Enemy
    {
        EnemyFSM creeperMode = EnemyFSM.Stroll;

        float health = 100f;


        public Creeper(Transform creeperObj)
        {
            base.enemyObj = creeperObj;
        }


        //Update the creeper's state
        public override void UpdateEnemy(Transform playerObj)
        {
            //The distance between the Creeper and the player
            float distance = (base.enemyObj.position - playerObj.position).magnitude;

            switch (creeperMode)
            {
                case EnemyFSM.Attack:
                    if (health < 20f)
                    {
                        creeperMode = EnemyFSM.Flee;
                    }
                    else if (distance > 2f)
                    {
                        creeperMode = EnemyFSM.MoveTowardsPlayer;
                    }
                    break;
                case EnemyFSM.Flee:
                    if (health > 60f)
                    {
                        creeperMode = EnemyFSM.Stroll;
                    }
                    break;
                case EnemyFSM.Stroll:
                    if (distance < 10f)
                    {
                        creeperMode = EnemyFSM.MoveTowardsPlayer;
                    }
                    break;
                case EnemyFSM.MoveTowardsPlayer:
                    if (distance < 1f)
                    {
                        creeperMode = EnemyFSM.Attack;
                    }
                    else if (distance > 15f)
                    {
                        creeperMode = EnemyFSM.Stroll;
                    }
                    break;
            }

            //Move the enemy based on a state
            DoAction(playerObj, creeperMode);
        }
    }
}

```

Luego de agregar el Script GameController a un GameObject Empty y agregar los elementos en cada una de las variables, ejecutamos y podemos ver su comportamiento, para poder ver cada uno de los estados en funcionamiento lo que debemos hacer es mover nuestra Esfera que hace el papel del player, para que esto active cada uno de los estados de los enemigos.

Vamos a poder ver como el Skeleton se queda un poco alejado del player, mientras que el Creeper se acerca al jugador, cuando nos alejamos empiezan a girar en su punto simulando una ronda o patrullaje.

![](StatePatternExample.gif)

<a name="SubClassPattern"></a>
## 05 - SubClass SandBox Pattern

El patrón Subclass Sandbox Pattern describe una idea básica, este es muy útil cuando tenemos varias subclases similares, cuando necesitamos hacer un pequeño cambio, lo que hacemos es cambiar la clase base, mientras que todas las subclases no necesitan ser tocadas. Entonces la clase base provee todas las operaciones a las clases derivadas.

Para este ejemplo solo necesitamos dos scripts, un GameController y un Superpower, el cual va a incluir varias clases.

### GameController Class:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SubclassSandbox
{
    public class GameController : MonoBehaviour
    {
        //A list that will store all superpowers
        List<Superpower> superPowers = new List<Superpower>();

        void Start()
        {
            superPowers.Add(new SkyLaunch());
            superPowers.Add(new GroundDive());
        }

        void Update()
        {
            //Activate each superpower each update
            for (int i = 0; i < superPowers.Count; i++)
            {
                superPowers[i].Activate();
            }
        }
    }
}

```

### Superpower Class:

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SubclassSandbox
{
    //This is the base class
    public abstract class Superpower
    {
        //This is the sandbox method that a subclass has to have its own version of
        public abstract void Activate();

        //All of the operations a derived class needs to perform - called from Activate()
        protected void Move(float speed)
        {
            Debug.Log("Moving with speed " + speed);
        }

        protected void PlaySound(string coolSound)
        {
            Debug.Log("Playing sound " + coolSound);
        }

        protected void SpawnParticles()
        {

        }
    }


    //Subclasses
    public class SkyLaunch : Superpower
    {
        //Has to have its own version of Activate()
        public override void Activate()
        {
            //Add operations this class has to perform
            Move(10f);
            PlaySound("SkyLaunch");
            SpawnParticles();
        }
    }

    public class GroundDive : Superpower
    {
        //Has to have its own version of Activate()
        public override void Activate()
        {
            //Add operations this class has to perform
            Move(15f);
            PlaySound("GroundDive");
            SpawnParticles();
        }
    }
}

```

Si agregamos el script GameController a un objeto vacío en Unity y presionamos Play, vamos a ver que no sucede nada, excepto por los mensajes que aparecen en la consola de Unity, con esto podemos ver la idea general de este patrón y cómo implementarlo.

![](ConsoleSubClass.png)

<a name="SpatialPattern"></a>
## 06 - Spatial Partition Pattern

Cuando hacemos un juego, muchas veces necesitamos encontrar la posición del enemigo que está más cerca al jugador. La manera más común de hacer esto es almacenar todos los enemigos en una lista, buscar en la lista todos los enemigos y por cada enemigo calcular la distancia al jugador. Este código en C# encuentra al enemigo más cercano:

```csharp
//Soldier is a friendly soldier
GameObject FindClosestEnemySlow(GameObject soldier)
{
	GameObject closestEnemy = null;

	float bestDistSqr = Mathf.Infinity;

	//Loop through all enemies
	for (int i = 0; i < enemySoldiers.Count; i++)
	{
		//The distance sqr between the soldier and this enemy
		float distSqr = (soldier.transform.position - enemySoldiers[i].transform.position).sqrMagnitude;

		//If this distance is better than the previous best distance, then we have found an enemy that's closer
		if (distSqr < bestDistSqr)
		{
			bestDistSqr = distSqr;

			closestEnemy = enemySoldiers[i];
		}
	}

	return closestEnemy;
}
```

Esto puede funcionar si uno tiene pocos enemigos, pero si uno tiene un campo de batalla con muchos soldados y enemigos, por cada soldado amigo tenemos que buscar en la lista de todos los enemigos, si tenemos 100 soldados amigos, vamos a tener que repetir el cálculo 100 veces.

Una mejor forma es almacenar todos los enemigos en una estructura de datos espacial que organice los objetos por sus posiciones. Podemos almacenar ambos objetos movibles, pero también objetos estáticos como obstáculos en esta estructura de datos. Una solución común es dividir el área en un grid 2D luego asociamos cada celda a los enemigos en ella. Luego para encontrar el enemigo más cercano, necesitamos saber en cual celda está el player y luego obtener los enemigos en esa celda, luego buscamos cuál de los enemigos en la celda es el más cercano al jugador.

La idea acá es que vamos a tener esferas cazando cubos, las esferas son amigas mientras que los cubos son llamados enemigos. En Unity agregamos un objeto vacío llamado GameController, luego agregamos dos objetos vacíos llamado Enemy Soldiers y Friendly Soldiers a los cuales vamos a emparentar los cubos y las esferas para tener un espacio de trabajo limpio, luego agregamos el terreno donde van a estar todo los elementos. Para que esto funcione la esquina del plano debe empezar en la posición (0,0,0), para esto la posición debe ser (25,0,25) y la escala (5,1,5). La razón de esto es que debemos trasladar las coordenadas de los cubos y las esferas a posiciones en el grid, es más fácil si todo inicia en (0,0,0). Todo se debe ver como en la siguiente imagen:

![](ConfigSpatial.png)

También necesitamos una esfera y un cubo, ambos deben ser prefabs. También necesitamos materiales, la razón de esto es que vamos a cambiar el material del cubo para detectar cual de ellos está más cerca.

### Los Script para la partición espacial.

Ahora necesitamos agregar un nuevo script llamado GameController. Este Script agrega nuevos cubos y esferas de forma aleatoria en el mapa, este va manejar el cambio de los colores y también incluye la versión lenta para encontrar el enemigo más cercano, agregamos lo siguiente:

### GameController Script:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    public class GameController : MonoBehaviour
    {
        public GameObject friendlyObj;
        public GameObject enemyObj;

        //Change materials to detect which enemy is the closest
        public Material enemyMaterial;
        public Material closestEnemyMaterial;

        //To get a cleaner workspace, parent all soldiers to these empty gameobjects
        public Transform enemyParent;
        public Transform friendlyParent;

        //Store all soldiers in these lists
        List<Soldier> enemySoldiers = new List<Soldier>();
        List<Soldier> friendlySoldiers = new List<Soldier>();

        //Save the closest enemies to easier change back its material
        List<Soldier> closestEnemies = new List<Soldier>();

        //Grid data
        float mapWidth = 50f;
        int cellSize = 10;

        //Number of soldiers on each team
        int numberOfSoldiers = 100;

        //The Spatial Partition grid
        Grid grid;


        void Start()
        {
            //Create a new grid
            grid = new Grid((int)mapWidth, cellSize);

            //Add random enemies and friendly and store them in a list
            for (int i = 0; i < numberOfSoldiers; i++)
            {
                //Give the enemy a random position
                Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                //Create a new enemy
                GameObject newEnemy = Instantiate(enemyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the enemy to a list
                enemySoldiers.Add(new Enemy(newEnemy, mapWidth, grid));

                //Parent it
                newEnemy.transform.parent = enemyParent;


                //Give the friendly a random position
                randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                //Create a new friendly
                GameObject newFriendly = Instantiate(friendlyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the friendly to a list
                friendlySoldiers.Add(new Friendly(newFriendly, mapWidth));

                //Parent it 
                newFriendly.transform.parent = friendlyParent;
            }
        }


        void Update()
        {
            //Move the enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                enemySoldiers[i].Move();
            }

            //Reset material of the closest enemies
            for (int i = 0; i < closestEnemies.Count; i++)
            {
                closestEnemies[i].soldierMeshRenderer.material = enemyMaterial;
            }

            //Reset the list with closest enemies
            closestEnemies.Clear();

            //For each friendly, find the closest enemy and change its color and chase it
            for (int i = 0; i < friendlySoldiers.Count; i++)
            {
                //Soldier closestEnemy = FindClosestEnemySlow(friendlySoldiers[i]);

                //The fast version with spatial partition
                Soldier closestEnemy = grid.FindClosestEnemy(friendlySoldiers[i]);

                //If we found an enemy
                if (closestEnemy != null)
                {
                    //Change material
                    closestEnemy.soldierMeshRenderer.material = closestEnemyMaterial;

                    closestEnemies.Add(closestEnemy);

                    //Move the friendly in the direction of the enemy
                    friendlySoldiers[i].Move(closestEnemy);
                }
            }
        }


        //Find the closest enemy - slow version
        Soldier FindClosestEnemySlow(Soldier soldier)
        {
            Soldier closestEnemy = null;

            float bestDistSqr = Mathf.Infinity;

            //Loop thorugh all enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                //The distance sqr between the soldier and this enemy
                float distSqr = (soldier.soldierTrans.position - enemySoldiers[i].soldierTrans.position).sqrMagnitude;

                //If this distance is better than the previous best distance, then we have found an enemy that's closer
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;

                    closestEnemy = enemySoldiers[i];
                }
            }

            return closestEnemy;
        }
    }
}

```

Luego están los Soldados, estos heredan de una clase Soldier

### Soldier Class:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    //The soldier base class for enemies and friendly
    public class Soldier
    {
        //To change material
        public MeshRenderer soldierMeshRenderer;
        //To move the soldier
        public Transform soldierTrans;
        //The speed the soldier is walking with
        protected float walkSpeed;
        //Has to do with the grid, so we can avoid storing all soldiers in an array
        //Instead we are going to use a linked list where all soldiers in the cell 
        //Are linked to each other
        public Soldier previousSoldier;
        public Soldier nextSoldier;

        //The enemy doesnt need any outside information
        public virtual void Move()
        { }

        //The friendly has to move which soldier is the closest
        public virtual void Move(Soldier soldier)
        { }
    }
}

```

### The enemy cubes class:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    //The enemy cube being chased by the spheres
    public class Enemy : Soldier
    {
        //The position the soldier is heading for when moving
        Vector3 currentTarget;
        //The position the soldier had before it moved, so we can see if it should change cell
        Vector3 oldPos;
        //The width of the map to generate random coordinated within the map
        float mapWidth;
        //The grid
        Grid grid;


        //Init enemy
        public Enemy(GameObject soldierObj, float mapWidth, Grid grid)
        {
            //Save what we need to save
            this.soldierTrans = soldierObj.transform;

            this.soldierMeshRenderer = soldierObj.GetComponent<MeshRenderer>();

            this.mapWidth = mapWidth;

            this.grid = grid;

            //Add this unit to the grid
            grid.Add(this);

            //Init the old pos
            oldPos = soldierTrans.position;

            this.walkSpeed = 5f;

            //Give it a random coordinate to move towards
            GetNewTarget();
        }


        //Move the cube randomly across the map
        public override void Move()
        {
            //Move towards the target
            soldierTrans.Translate(Vector3.forward * Time.deltaTime * walkSpeed);

            //See if the the cube has moved to another cell
            grid.Move(this, oldPos);

            //Save the old position
            oldPos = soldierTrans.position;

            //If the soldier has reached the target, find a new target
            if ((soldierTrans.position - currentTarget).magnitude < 1f)
            {
                GetNewTarget();
            }
        }


        //Give the enemy a new target to move towards and rotate towards that target
        void GetNewTarget()
        {
            currentTarget = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

            //Rotate towards the target
            soldierTrans.rotation = Quaternion.LookRotation(currentTarget - soldierTrans.position);
        }
    }
}

```
### The friendly spheres:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    //The friendly sphere which is chasing the enemy cubes
    public class Friendly : Soldier
    {
        //init friendly
        public Friendly(GameObject soldierObj, float mapWidth)
        {
            this.soldierTrans = soldierObj.transform;

            this.walkSpeed = 2f;
        }


        //Move towards the closest enemy - will always move within its grid
        public override void Move(Soldier closestEnemy)
        {
            //Rotate towards the closest enemy
            soldierTrans.rotation = Quaternion.LookRotation(closestEnemy.soldierTrans.position - soldierTrans.position);
            //Move towards the closest enemy
            soldierTrans.Translate(Vector3.forward * Time.deltaTime * walkSpeed);
        }
    }
}

```

Si pensamos un poco, es la esfera amiga la que casa a los cubos enemigos, pero solo cuando los cubos se están moviendo aleatoriamente, las esferas las que actualmente son el enemigo. La parte un poco confusa es que no estamos almacenando los soldados que est´na es cada celda en un array, en lugar estan en una linkedlist, entonces cada soldados está referenciando a otro soldado en la misma lista, luego otro soldado se refiere a otro soldado y así sucesivamente.

### Grid Class:
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    public class Grid
    {
        //Need this to convert from world coordinate position to cell position
        int cellSize;

        //This is the actual grid, where a soldier is in each cell
        //Each individual soldier links to other soldiers in the same cell
        Soldier[,] cells;


        //Init the grid
        public Grid(int mapWidth, int cellSize)
        {
            this.cellSize = cellSize;

            int numberOfCells = mapWidth / cellSize;

            cells = new Soldier[numberOfCells, numberOfCells];
        }


        //Add a unity to the grid
        public void Add(Soldier soldier)
        {
            //Determine which grid cell the soldier is in
            int cellX = (int)(soldier.soldierTrans.position.x / cellSize);
            int cellZ = (int)(soldier.soldierTrans.position.z / cellSize);

            //Add the soldier to the front of the list for the cell it's in
            soldier.previousSoldier = null;
            soldier.nextSoldier = cells[cellX, cellZ];

            //Associate this cell with this soldier
            cells[cellX, cellZ] = soldier;

            if (soldier.nextSoldier != null)
            {
                //Set this soldier to be the previous soldier of the next soldier of this soldier (linked lists ftw)
                soldier.nextSoldier.previousSoldier = soldier;
            }
        }


        //Get the closest enemy from the grid
        public Soldier FindClosestEnemy(Soldier friendlySoldier)
        {
            //Determine which grid cell the friendly soldier is in
            int cellX = (int)(friendlySoldier.soldierTrans.position.x / cellSize);
            int cellZ = (int)(friendlySoldier.soldierTrans.position.z / cellSize);

            //Get the first enemy in grid
            Soldier enemy = cells[cellX, cellZ];

            //Find the closest soldier of all in the linked list
            Soldier closestSoldier = null;

            float bestDistSqr = Mathf.Infinity;

            //Loop through the linked list
            while (enemy != null)
            {
                //The distance sqr between the soldier and this enemy
                float distSqr = (enemy.soldierTrans.position - friendlySoldier.soldierTrans.position).sqrMagnitude;

                //If this distance is better than the previous best distance, then we have found an enemy that's closer
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;

                    closestSoldier = enemy;
                }

                //Get the next enemy in the list
                enemy = enemy.nextSoldier;
            }

            return closestSoldier;
        }


        //A soldier in the grid has moved, so see if we need to update in which grid the soldier is
        public void Move(Soldier soldier, Vector3 oldPos)
        {
            //See which cell it was in 
            int oldCellX = (int)(oldPos.x / cellSize);
            int oldCellZ = (int)(oldPos.z / cellSize);

            //See which cell it is in now
            int cellX = (int)(soldier.soldierTrans.position.x / cellSize);
            int cellZ = (int)(soldier.soldierTrans.position.z / cellSize);

            //If it didn't change cell, we are done
            if (oldCellX == cellX && oldCellZ == cellZ)
            {
                return;
            }

            //Unlink it from the list of its old cell
            if (soldier.previousSoldier != null)
            {
                soldier.previousSoldier.nextSoldier = soldier.nextSoldier;
            }

            if (soldier.nextSoldier != null)
            {
                soldier.nextSoldier.previousSoldier = soldier.previousSoldier;
            }

            //If it's the head of a list, remove it
            if (cells[oldCellX, oldCellZ] == soldier)
            {
                cells[oldCellX, oldCellZ] = soldier.nextSoldier;
            }

            //Add it bacl to the grid at its new cell
            Add(soldier);
        }
    }
}

```


Si agregamos todos los script y los elementos necesarios, al presionar play vamos a ver un funcionamiento similar a este:

![](Spatial.gif)

Podemos ver que cada esfera no está casando cada cubo si no tiene un cubo en su celda, este es un efecto secundario muy interesante que se puede usar para hacer una patrulla de objetos.




Todo el contenido fue traducido de la siguiente página: [Habrador](https://www.habrador.com/tutorials/programming-patterns/1-command-pattern/)
    
