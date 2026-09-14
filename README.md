# Vigilance

A two-layer anti-cheat system for multiplayer games, built as part of a bachelor's thesis.

## Overview

Vigilance combines two complementary defense layers:

- **Layer 1 — Server-Authoritative Architecture**: The client only sends raw input events (key presses, mouse movement). All game logic, state, and outcomes are computed exclusively on the server, structurally eliminating state-based cheats (e.g. RAM manipulation, score/position tampering).
- **Layer 2 — Behavioral Cheat Detection**: A neural network trained on player input telemetry detects behavioral cheats (e.g. aimbots) that are indistinguishable from legitimate input at the server-verification level.

## Project Structure

- `/client` — Unity client (input capture, rendering)
- `/server` — Authoritative game server
- `/ml` — Neural network training pipeline and models
- `/data` — Collected input telemetry (not committed — see `.gitignore`)

## Status

Work in progress — bachelor's thesis project, SAE Institute Zürich.

## Author

Yannic Sutter
