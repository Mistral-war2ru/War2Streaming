# War2Streaming
Warcraft2 game chat integration with twitch and youtube(experimental)

game plugin + program

plugin set 2 buffers for messages.
every game tick it checking if receiving buffer not empty it shows message in game chat.
if player writes something in chat plugin put it into sending buffer.

program using twitch auth key to connect to channel chat and listening for messages.

you can get twitch auth here: https://twitchapps.com/tmi/

messages saved in list then sended to game plugin buffer one by one.
and also program checking game plugin sending buffer and resend message from it to twitch channel chat.

additionally streamer can set commands for viewers too add/remove themselves to list of names.
those names can be shown on top of newly created units, so basically it allows to viewers take part in your game.

program using TwitchLib for C# - https://github.com/TwitchLib/TwitchLib
program using GoogleAPI for youtube integration https://www.nuget.org/packages/Google.Apis
