#!/bin/sh

DEST="/usr/local/DiffScribe"

case "$SHELL" in
    */zsh)   PROFILE="$HOME/.zprofile"      ;; # zsh
    */bash)  PROFILE="$HOME/.bash_profile"  ;; # bash
    */ksh)   PROFILE="$HOME/.profile"       ;; # ksh / sh
    */fish)  PROFILE="$HOME/.config/fish/config.fish" ;; # fish shell
    *)       PROFILE="$HOME/.profile"       ;; # fallback
esac

if grep -qs "$DEST" "$PROFILE"; then
    sed -i.bak '/DiffScribe/d' "$PROFILE" && rm "$PROFILE.bak"
    echo "$DEST removed from PATH. Restart your shell session."
else
    echo "$DEST not found in PATH."
fi

sudo rm -rf "$DEST"
echo "Application files are removed from $DEST."

echo "Uninstallation completed."