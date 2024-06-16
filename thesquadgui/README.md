# TheSquadGUI WPF Application

## Overview

TheSquad-Client (currently) is a Windows Presentation Foundation (WPF) application developed 
in C#. This application continually polls a local server endpoint of the live League of Legends
client to retrieve JSON data representing the champions currently selected by players during
champ select. It displays the champion IDs for two teams, each consisting of five players, in a simple and clear interface.

## Features

- **Continuous Polling:** The application continuously polls a specified local endpoint every 0.5 seconds to fetch updated champion data.
- **Dynamic UI Update:** The application dynamically updates the UI to reflect changes in the champion selection in real-time.
- **Simple and Clear Interface:** The user interface is designed to display the champion IDs in two columns representing the two teams, each with five rows for the respective players.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com/)
- [C# Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

## Getting Started

### Clone the Repository

"```bash"
git clone https://github.com/cjporter25/thesquad-client.git
cd thesquad-client

## Contributing
Feel free to contribute to this project by submitting issues or pull requests. For major changes, please open an issue first to discuss what you would like to change.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgements
Thanks to the .NET Core and WPF communities for providing great tools and documentation.
Special thanks to the developers of the libraries and tools used in this project.