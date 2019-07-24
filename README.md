# Unity-Programming-Patterns

### Un ejercicio de programación sobre como implementar patrones de programación en videojuegos.

En este tutorial se explica sobre los patrones de programación en Unity con C#, esta es la explicación que da Wikipedia sobre que es un patron de programación para los que no esten relacionados con el termino:

En ingenieria de software, un patron de diseño de software es una solución reusable para un problema que ocurre comunmente en un contexto especifico en el diseño de software. No es un diseño final que puede ser transformado directamente en código o lenguaje maquina. Es una descripción o template sobre como podemos resolver un problema, el cual puede ser usado en diferentes situaciones. Los patrones de diseño son buenas practicas que un programados puedes usar par resolver un problema común cuando de diseña una aplicación o un sistema.

## Command Pattern

En programación orientada a objetos, el Command Pattern es un patron de diseño de comportamiento en el cual un objeto es usado para encapsular toda la información necesaria para ejecutar una acción o activar algún evento en otro momento. Esta información incluye el nombre del método, el objeto que es propietario del método y los valores de los parametros del método.

Un ejemplo donde el Command Pattern es realmente útil, es cuando le damos la opción a nuestros usuarios de cambiar los controler del juego, en Unity podemos escribir una función similar a esta:
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
Luego FireWeapon hereda de la clase base. Neccesitamos una clase que no haga nada, de esta forma podemos hacer el Switch entre la U y la A cuando se dispare el arma.

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
Pero esto no es todo, Si estamos salvando los commands, estamos enviandolos a una lista, entonces podemos deshacer los commands. Digamos por ejemplo que tenemos una editor de niveles, en el cual estamos poniendo arboles. Luego podemos usar el Command Pattern para deshacer el ultimo o todos los comando en el caso que ubiquemos alguno en una posición erronea.

Al usar la misma lista cuando deshacemos algo podemos crear una función que haga un replay, si recordamos la posición inicial, simplemente recorremos los commands que salvamos y Unity va a reproducir todo lo que hicimos. Esta es una buena forma de salvar la posición y la orientación en cada frame de todos nuestros objetos en la escena, luego repetir las posiciones haciendo un loop en la lista.

### El Command Pattern en Unity

La idea en este script es que tenemos un box gameobject, luego podemos mover la caja usando las teclas WASD, pero gracias al command pattern podemos deshacer los moviemientos con la tecla Z, luego reproducir todos los movimientos desde el principio con la tecla R.

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

Todo el contenido fue traducido de la siguiente página: [Habrador](https://www.habrador.com/tutorials/programming-patterns/1-command-pattern/)
    }
