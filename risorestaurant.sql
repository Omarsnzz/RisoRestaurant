-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 14-04-2026 a las 23:13:01
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
('Chilaquiles Rojos', 110, 8),
('Chilaquiles Verdes', 110, 10),
('Chilaquiles Verdes - Pollo', 120, 10),
('Chilaquiles Rojos - Pollo', 120, 10);

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
(5, 'Coca-Cola', 60, 50);

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
  `propina` decimal(10,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `cuenta`
--

INSERT INTO `cuenta` (`idCuenta`, `Mesa`, `Mesero`, `Subtotal`, `PorcentajeDescuento`, `Total`, `FechaHora`, `propina`) VALUES
(1, 'General', 'General', 220.00, 0.00, 220.00, '2026-04-14 15:10:54', 0.00);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inventario`
--

CREATE TABLE `inventario` (
  `Nombre` varchar(100) NOT NULL,
  `Cantidad` decimal(10,2) NOT NULL,
  `UnidadMedida` varchar(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

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
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `bebidas`
--
ALTER TABLE `bebidas`
  MODIFY `idBebidas` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `cuenta`
--
ALTER TABLE `cuenta`
  MODIFY `idCuenta` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
