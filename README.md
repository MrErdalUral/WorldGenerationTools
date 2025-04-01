![image](https://github.com/MrErdalUral/WorldGenerationTools/blob/main/Screenshots/Screenshot.PNG?raw=true)

# Unity Procedural Island Generator

This Unity project demonstrates a procedural terrain generation system that creates unique island environments using a blend of advanced algorithms and custom shaders. The project is designed to showcase various techniques for terrain creation and rendering, making it an excellent starting point for further experimentation and development.

[DEMO on Itch](https://sagewind-studio.itch.io/island-generator)

## Features

- **Poisson Disc Sampling 2D Point Generation:**  
  Evenly distributes points across a 2D space to form a robust base for terrain features.

- **Triangle.Net Delaunay Triangulation:**  
  Uses Triangle.Net to triangulate the points, generating a mesh structure from the Poisson disc distribution.

- **Perlin Noise for Slope Determination:**  
  Reads Perlin noise to determine slopes between the Poisson Disc Graph nodes, adding realistic variations to the terrain.

- **Custom Triplanar Shader:**  
  Implements a custom triplanar shader for the final terrain mesh material, ensuring smooth texture blending regardless of the underlying geometry.

- **Water Surface Shader with Reflections:**  
  Features a water surface shader that adds dynamic reflections, enhancing the visual quality of water bodies within the scene.

- **Dither Punk Aesthetics:**  
  Custom shaders integrate a unique dither punk style for a striking, stylized look.

- **Simple Camera and Island Randomization Controls:**  
  Provides intuitive controls to navigate the scene and randomize island layouts, allowing for quick iterations and exploration.

## Getting Started

### Prerequisites

- [Unity](https://unity.com/) (Version 2022 or later recommended)
- (Optional) Additional dependencies or packages as needed

### Installation

1. **Clone the Repository:**

2. **Open the Project in Unity:**

   Launch Unity and open the cloned project folder.

3. **Run the Main Scene:**

   Open the test scene (e.g., `Scenes/Sample Scene (IslandGenerator).unity`) and press Play to see the procedural island generation in action.

## Usage

- **Terrain Settings:**  
  Modify the generation parameters (such as point density and noise scale) via the Unity Inspector to experiment with different island layouts and terrain variations.

- **Randomization Controls:**  
  Use the provided camera and island randomization controls to explore new configurations and perspectives.

- **Shader Customization:**  
  Tweak the custom triplanar and water surface shaders to adjust texture blending, reflections, and the overall dither punk aesthetic.

## Contributing

Contributions are welcome! Feel free to fork the repository, make your changes, and submit a pull request. If you have suggestions or encounter any issues, please open an issue in the repository.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/MrErdalUral/WorldGenerationTools/blob/main/LICENSE) file for more details.

## Acknowledgments

- [Unity](https://unity.com/)
- [Triangle.Net](https://triangle.codeplex.com/) for the triangulation library
- [Zenject](https://github.com/modesttree/Zenject)
- [R3](https://github.com/Cysharp/R3)
- [UniTask](https://github.com/Cysharp/UniTask)
