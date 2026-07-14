Español · [English](README.en.md) · [中文](README.md)

# Protocolo personal URL ?

El sistema permite a los desarrolladores crear protocolos URL personales para iniciar aplicaciones o entornos específicos. El formato general es `{protocol_name}:{launch_parameters}`.

Por ejemplo:

- PotPlayer: `potplayer:`
- WeChat: `weixin:`
- Microsoft VS Code: `vscode:`

Y así sucesivamente. Al abrir estos protocolos en un navegador, se iniciarán las aplicaciones correspondientes.

Sin embargo, algunos programas no proporcionan protocolos de URL personales por razones de seguridad, como VLC Player. En estos casos, este proyecto se hace necesario.

El principio de este proyecto es actuar como intermediario. Se registra como cualquier nombre de protocolo y, a continuación, selecciona la aplicación de destino. Cuando se abre un protocolo, busca la aplicación de destino correspondiente, procesa y corrige los parámetros, pasa los parámetros a la aplicación de destino y, a continuación, la inicia.

# Notas importantes

- `URLProtocol.exe` debe mantenerse en una ubicación fija y no debe borrarse ni moverse, ya que la funcionalidad de reenvío de protocolos depende de la ubicación del programa
- Si se traslada o elimina el programa, los protocolos personales registrados no funcionarán correctamente.

# Requisitos del sistema

Windows 10 1709 o posterior requiere [.NET Framework 4.7.2](https://go.microsoft.com/fwlink/?LinkId=863262).

# Uso

- Abrir `URLProtocol.exe` e introduzca el nombre del protocolo (excluyendo ":").
- Haga clic en Seleccionar programa de destino y seleccionar el archivo exe que desea llamar.
- Clic en Confirmar.

Comprobar si se ha realizado correctamente:

- Pruebe a abrir su protocolo personal con `:` en la barra de direcciones del navegador (ej., test:).
- Compruebe si el programa de destino se abre con normalidad.

Puede configurar el programa de llamada en la extensión [cat-catch](https://github.com/xifangczy/cat-catch).

# Prueba

Al llamar al protocolo, introduzca `--cat-catch-test` para mostrar la cadena de llamada final y seleccionar si continuar la llamada.
