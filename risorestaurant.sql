-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 11-05-2026 a las 21:43:31
-- Versión del servidor: 10.4.28-MariaDB
-- Versión de PHP: 8.2.4

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `risorestaurant`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `alimentos`
--

CREATE TABLE `alimentos` (
  `Nombre` varchar(30) NOT NULL,
  `Precio` int(11) NOT NULL,
  `Cantidad` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `alimentos`
--

INSERT INTO `alimentos` (`Nombre`, `Precio`, `Cantidad`) VALUES
('Chilaquiles Rojos', 110, 2),
('Chilaquiles Verdes', 110, 9),
('Chilaquiles Verdes - Pollo', 120, 9),
('Chilaquiles Rojos - Pollo', 120, 9);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `bebidas`
--

CREATE TABLE `bebidas` (
  `idBebidas` int(11) NOT NULL,
  `Nombre` varchar(45) DEFAULT NULL,
  `Costo` int(11) DEFAULT NULL,
  `Cantidad` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `bebidas`
--

INSERT INTO `bebidas` (`idBebidas`, `Nombre`, `Costo`, `Cantidad`) VALUES
(5, 'Coca-Cola', 60, 42),
(7, 'pepsi', 55, 3),
(8, 'coca cola 600 ml', 20, 4);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `cuenta`
--

CREATE TABLE `cuenta` (
  `idCuenta` int(11) NOT NULL,
  `Mesa` varchar(50) DEFAULT NULL,
  `Mesero` varchar(100) DEFAULT NULL,
  `Subtotal` decimal(10,2) DEFAULT NULL,
  `PorcentajeDescuento` decimal(5,2) DEFAULT NULL,
  `Total` decimal(10,2) DEFAULT NULL,
  `FechaHora` datetime DEFAULT current_timestamp(),
  `propina` decimal(10,2) NOT NULL,
  `Articulos` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `cuenta`
--

INSERT INTO `cuenta` (`idCuenta`, `Mesa`, `Mesero`, `Subtotal`, `PorcentajeDescuento`, `Total`, `FechaHora`, `propina`, `Articulos`) VALUES
(7, '2', 'juan', 280.00, 0.00, 280.00, '2026-05-04 10:50:07', 0.00, 'alimentos|Chilaquiles Rojos|1|110|110~bebidas|Coca-Cola|1|60|60~alimentos|Chilaquiles Verdes|1|110|110');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inventario`
--

CREATE TABLE `inventario` (
  `Nombre` varchar(100) NOT NULL,
  `Cantidad` decimal(10,2) NOT NULL,
  `UnidadMedida` varchar(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pedidos_domicilio`
--

CREATE TABLE `pedidos_domicilio` (
  `Folio` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Telefono` varchar(20) NOT NULL,
  `Lugar` varchar(200) NOT NULL,
  `Comida` varchar(255) NOT NULL,
  `Dia` date NOT NULL,
  `Hora` varchar(10) NOT NULL,
  `Total` decimal(10,2) NOT NULL,
  `Estado` varchar(30) DEFAULT 'Pendiente'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `pedidos_domicilio`
--

INSERT INTO `pedidos_domicilio` (`Folio`, `Nombre`, `Telefono`, `Lugar`, `Comida`, `Dia`, `Hora`, `Total`, `Estado`) VALUES
(3, 'Pedro', '14455777', 'Alvarez #4', 'Chilaquiles Verdes - Pollo', '2026-04-20', '7:00 pm', 120.00, 'Entregado'),
(4, 'Alfredo', '7331445588', 'Alvarez #3', 'Chilaquiles Rojos - Pollo, Coca-Cola', '2026-04-21', '10:00', 180.00, 'Entregado'),
(6, 'sdsad', '8544', 'asdas', '1 x Chilaquiles Rojos - Pollo', '2026-04-21', '554', 120.00, 'Pendiente');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `bebidas`
--
ALTER TABLE `bebidas`
  ADD PRIMARY KEY (`idBebidas`);

--
-- Indices de la tabla `cuenta`
--
ALTER TABLE `cuenta`
  ADD PRIMARY KEY (`idCuenta`);

--
-- Indices de la tabla `pedidos_domicilio`
--
ALTER TABLE `pedidos_domicilio`
  ADD PRIMARY KEY (`Folio`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `bebidas`
--
ALTER TABLE `bebidas`
  MODIFY `idBebidas` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT de la tabla `cuenta`
--
ALTER TABLE `cuenta`
  MODIFY `idCuenta` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `pedidos_domicilio`
--
ALTER TABLE `pedidos_domicilio`
  MODIFY `Folio` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
