# War2Twitch
Warcraft2 game chat integration with twitch.
game plugin + program

plugin set 2 buffers for messages.
every game tick it checking if receiving buffer not empty it shows message in game chat.
if player writes something in chat plugin put it into sending buffer.

program using twitch auth key to connect to channel chat and listening for messages.
messages saved in list then sended to game plugin buffer one by one.
and also program checking game plugin sending buffer and resend message from it to twitch channel chat.

program using TwitchLib for C# - https://github.com/TwitchLib/TwitchLib
