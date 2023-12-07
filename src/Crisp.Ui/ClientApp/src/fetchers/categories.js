export const fetchCategory = async () => {
    const response = await fetch('api/categories');
    const result = await response.json();
    if (response.status !== 200) {
        throw Error(result.detail);
    }
    return result;
}