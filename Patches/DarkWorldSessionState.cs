// #pragma warning disable CS0626
// #pragma warning disable CS0108
// using MonoMod;

// namespace TowerFall;

// public class patch_DarkWorldSessionState : DarkWorldSessionState
// {
//     public int[] Points;
//     public patch_DarkWorldSessionState(Session session) : base(session) {}

//     public extern void orig_ctor(Session session);

//     [MonoModConstructor]
//     public void ctor(Session session) 
//     {
//         orig_ctor(session);
//         Points = new int[4];
//     }
// }