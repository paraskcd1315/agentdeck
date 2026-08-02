pub(crate) fn quote_argument(value: &str) -> String {
    if !value.is_empty() && !value.contains([' ', '\t', '"']) {
        return value.to_string();
    }

    let mut quoted = String::with_capacity(value.len() + 2);
    quoted.push('"');

    let mut pending_backslashes = 0usize;
    for character in value.chars() {
        match character {
            '\\' => pending_backslashes += 1,
            '"' => {
                quoted.extend(std::iter::repeat_n('\\', pending_backslashes * 2 + 1));
                pending_backslashes = 0;
                quoted.push('"');
            }
            _ => {
                quoted.extend(std::iter::repeat_n('\\', pending_backslashes));
                pending_backslashes = 0;
                quoted.push(character);
            }
        }
    }

    quoted.extend(std::iter::repeat_n('\\', pending_backslashes * 2));
    quoted.push('"');
    quoted
}

pub(crate) fn build_command_line<'a>(parts: impl Iterator<Item = &'a str>) -> String {
    parts
        .map(quote_argument)
        .collect::<Vec<_>>()
        .join(" ")
}
