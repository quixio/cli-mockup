#!/bin/bash
gnome-terminal -- /bin/bash -c "source $1/venv/bin/activate; exec /bin/bash"