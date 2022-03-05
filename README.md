# Word Decoder API

## Overview

Word Decoder API is a game where players have six chances to guess a five-letter word.

The API should have two functions - to allow players to start a new game by setting a word, and to allow players to submit guesses against the active word.

## Game Flow / Rules

A list of valid words that may be used by players when setting a word or submitting guesses can be found in the [wordList.csv](WordDecoderApi/WordDecoder/Data/wordList.csv).

A valid five-letter word is set using the API, starting the game.

A valid five-letter word is submitted as a guess using the API.

They receive an affirmative response to the request that contains:
- The results of the players guess. For each letter in the guess, the player should be informed whether:
    - The letter is in the word and in the correct spot, or,
    - The letter is in the word and in the incorrect spot, or,
    - The letter is not in the word at all

- If the guess word is equal to the game word, the game ends and the players win.
- If six guesses have occurred without successfully guessing the game word, the game ends and the players lose

### Examples

X = letter is not in word

H =letter is in word, but incorrect spot

Y = letter is in word, in correct spot

If the game word is YEAST:
- guess is QUILL: response is X X X X X
- guess is QUILT: response is X X X X Y
- guess is QUIET: response is X X X H Y
- guess is BLAST: response is X X Y Y Y
- guess is YEAST: response is Y Y Y Y Y & game ends (players win)

If the game word is LEVEL:
- guess is PASTE: response is X X X X H
- guess is FEEDS: response is X Y H X X
- guess is FEELS: response is X Y H H X
- guess is FEVER: response is X Y Y Y X
- guess is LEVER: response is Y Y Y Y X
- guess is LEVEL: response is Y Y Y Y Y & game ends (players win)

If the game word is RADII:
- guess is FEVER: response is X X X X X
- guess is QUEEN: response is X X X X X
- guess is MOONS: response is X X X X X
- guess is RESTS: response is Y X X X X
- guess is TEPID: response is X X X Y H
- guess is RADAR: response is Y Y Y X X & game ends (players lose)

## Tech Stack

- ASP.NET 6 Minimal API
- Swagger/OpenAPI
- In-Memory EF Core 6
- xUnit

## Get Started

- Open the solution on Visual Studio
- Build and Restore Package
- Startup from WordDecoderApi project
- While in devopment environment you can consume the API through the Swagger UI
- Alternatively download [Postman](https://www.postman.com/downloads/) and import the [WordDecoder collection](WordDecoder.postman_collection.json).