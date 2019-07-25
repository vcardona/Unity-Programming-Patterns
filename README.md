# Unity-Programming-Patterns

### Un ejercicio de programación sobre como implementar patrones de programación en videojuegos.

En este tutorial se explican algunos de los patrones de programación en Unity con C#, esta es la explicación que da Wikipedia sobre que es un patrón de programación para los que no esten relacionados con el termino:

En ingenieria de software, un patrón de diseño de software es una solución reusable para un problema que ocurre comunmente en un contexto especifico en el diseño de software. No es un diseño final que puede ser transformado directamente en código o lenguaje maquina. Es una descripción o template sobre como podemos resolver un problema, el cual puede ser usado en diferentes situaciones. Los patrones de diseño son buenas practicas que un programador puedes usar par resolver un problema común cuando de diseña una aplicación o un sistema.

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


Todo el contenido fue traducido de la siguiente página: [Habrador](https://www.habrador.com/tutorials/programming-patterns/1-command-pattern/)
    
